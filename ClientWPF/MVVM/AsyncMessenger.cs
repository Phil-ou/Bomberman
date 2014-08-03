using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ClientWPF.MVVM
{
    public class AsyncMessengerAggregateException : Exception
    {
        public ReadOnlyCollection<Exception> InnerExceptions { get; private set; }

        public AsyncMessengerAggregateException(string message, IList<Exception> exceptions)
            : base(message, (exceptions != null) && (exceptions.Count > 0) ? exceptions[0] : null)
        {
            if (exceptions == null)
            {
                throw new ArgumentNullException("exceptions");
            }
            if (exceptions.Any(ex => ex == null))
            {
                throw new ArgumentException();
            }
            InnerExceptions = new ReadOnlyCollection<Exception>(exceptions);
        }

        public AsyncMessengerAggregateException(string message, Exception innerException)
            : base(message, innerException)
        {
            if (innerException == null)
            {
                throw new ArgumentNullException("innerException");
            }
            InnerExceptions = new ReadOnlyCollection<Exception>(new[] { innerException });
        }

        public override string ToString()
        {
            string str = base.ToString();
            for (int i = 0; i < InnerExceptions.Count; i++)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}{1}---> (Inner Exception #{2}) {3}<--{4}", str, Environment.NewLine, i, InnerExceptions[i], Environment.NewLine);
            }
            return str;
        }
    }

    public static class AsyncMessenger
    {
        private class Subscription
        {
            private WeakReference RecipientWeakReference { get; set; }

            public object Recipient
            {
                get
                {
                    if (RecipientWeakReference == null)
                        return null;
                    return RecipientWeakReference.Target;
                }
            }

            public object Token { get; private set; }
            public MethodInfo Method { get; private set; }
            public SynchronizationContext Context { get; private set; }

            public bool IsAlive
            {
                get
                {
                    if (RecipientWeakReference == null)
                        return false;
                    return RecipientWeakReference.IsAlive;
                }
            }

            public Subscription(object recipient, MethodInfo method, object token, SynchronizationContext context)
            {
                RecipientWeakReference = new WeakReference(recipient);
                Method = method;
                Token = token;
                Context = context;
            }
        }

        private static readonly Dictionary<Type, List<Subscription>> Recipients = new Dictionary<Type, List<Subscription>>();

        #region Register

        public static void Register<T>(object recipient, Action<T> action)
        {
            if (IsInDesignModeStatic)
                return;
            if (recipient == null)
                throw new ArgumentNullException("recipient");
            if (action == null)
                throw new ArgumentNullException("action");
            Register(recipient, null, action);
        }

        public static void Register<T>(object recipient, object token, Action<T> action)
        {
            if (IsInDesignModeStatic)
                return;
            if (recipient == null)
                throw new ArgumentNullException("recipient");
            if (action == null)
                throw new ArgumentNullException("action");
            lock (Recipients)
            {
                Type messageType = typeof (T);

                List<Subscription> subscriptions;
                if (!Recipients.ContainsKey(messageType))
                {
                    subscriptions = new List<Subscription>();
                    Recipients.Add(messageType, subscriptions);
                }
                else
                    subscriptions = Recipients[messageType];

                lock (subscriptions)
                {
                    subscriptions.Add(new Subscription(recipient, action.Method, token, SynchronizationContext.Current));
                }
            }
            Cleanup();
        }

        #endregion

        #region Unregister

        public static void Unregister(object recipient)
        {
            if (IsInDesignModeStatic)
                return;
            lock (Recipients)
            {
                foreach (KeyValuePair<Type, List<Subscription>> kv in Recipients)
                    UnregisterFromList(kv.Value, x => x.Recipient == recipient);
            }
        }

        public static void Unregister<T>(object recipient)
        {
            if (IsInDesignModeStatic)
                return;
            lock (Recipients)
            {
                Type messageType = typeof (T);
                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient);
            }
        }

        public static void Unregister<T>(object recipient, Action<T> action)
        {
            if (IsInDesignModeStatic)
                return;
            lock (Recipients)
            {
                Type messageType = typeof (T);
                MethodInfo method = action.Method;

                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient && x.Method == method);
            }
        }

        public static void Unregister<T>(object recipient, object token)
        {
            if (IsInDesignModeStatic)
                return;
            lock (Recipients)
            {
                Type messageType = typeof (T);

                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient && x.Token == token);
            }
        }

        public static void Unregister<T>(object recipient, object token, Action<T> action)
        {
            lock (Recipients)
            {
                Type messageType = typeof (T);
                MethodInfo method = action.Method;

                if (Recipients.ContainsKey(messageType))
                    UnregisterFromList(Recipients[messageType], x => x.Recipient == recipient && x.Method == method && x.Token == token);
            }
        }

        //
        private static void UnregisterFromList(List<Subscription> list, Func<Subscription, bool> filter)
        {
            lock (list)
            {
                List<Subscription> toRemove = list.Where(filter).ToList();
                foreach (Subscription item in toRemove)
                    list.Remove(item);
            }
            Cleanup();
        }

        #endregion

        #region Send

        public static void Send<T>(T message)
        {
            if (IsInDesignModeStatic)
                return;
            Send(message, null);
        }

        public static void Send<T>(T message, object token)
        {
            if (IsInDesignModeStatic)
                return;
            List<Subscription> clone = null;
            lock (Recipients)
            {
                Type messageType = typeof (T);

                if (Recipients.ContainsKey(messageType))
                {
                    // Clone to avoid problem if register/unregistering in "receive message" method
                    lock (Recipients[messageType])
                    {
                        clone = Recipients[messageType].Where(x => (x.Token == null && token == null)
                                                                   ||
                                                                   (x.Token != null && x.Token.Equals(token))
                            ).ToList();
                    }
                }
            }
            if (clone != null)
                SendToList(clone, message);
        }

        private static void SendToList<T>(IEnumerable<Subscription> list, T message)
        {
            // Send message to matching recipients
            List<Exception> exceptions = new List<Exception>();
            foreach (Subscription item in list)
            {
                try
                {
                    if (item.IsAlive)
                    {
                        //http://stackoverflow.com/questions/4843010/net-how-do-i-invoke-a-delegate-on-a-specific-thread-isynchronizeinvoke-disp
                        // Execute on thread which performed Register if possibe
                        Subscription subscription = item; // avoid closure problem
                        if (subscription.Context != null)
                        {
                            subscription.Context.Post(
                                _ => subscription.Method.Invoke(subscription.Recipient, new object[] {message}), null);
                        }
                        else // no context specified while registering, create a delegate and BeginInvoke
                        {
                            Func<object, object[], object> delegateToMethod = subscription.Method.Invoke;
                            delegateToMethod.BeginInvoke(subscription.Recipient, new object[] {message}, null, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (exceptions.Any())
                throw new AsyncMessengerAggregateException("Error with SendToList", exceptions);
            Cleanup();
        }

        #endregion

        #region Cleanup

        private static void Cleanup()
        {
            // Clean dead recipients
            lock (Recipients)
            {
                foreach (KeyValuePair<Type, List<Subscription>> kv in Recipients)
                {
                    List<Subscription> list = kv.Value;
                    List<Subscription> toRemove = list.Where(x => !x.IsAlive || x.Recipient == null).ToList();
                    foreach (Subscription item in toRemove)
                        list.Remove(item);
                }
            }
        }

        #endregion

        #region Design Time
        //http://blog.galasoft.ch/posts/2009/09/detecting-design-time-mode-in-wpf-and-silverlight/

        private static bool? _isInDesignMode;
        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
            _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool) DependencyPropertyDescriptor
                                     .FromProperty(prop, typeof (FrameworkElement))
                                     .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }

        #endregion
    }
}
