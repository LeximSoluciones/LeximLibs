using System;

namespace Lexim.Utils
{
    public static class NullableExtensions
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static void Do<T>(this T obj, Action<T> onSome, Action onNone = null) where T : class
        {
            if (obj != null)
            {
                onSome(obj);
            }
            else
            {
                onNone?.Invoke();
            }
        }

        public static TResult Map<T, TResult>(this T option, Func<T, TResult> onSome, Func<TResult> onNone) where T : class
        {
            return
                option != null
                    ? onSome(option)
                    : onNone();
        }

        public static TResult MapOrDefault<T, TResult>(this T option, Func<T, TResult> onSome) where T : class =>
            option.Map(onSome, () => default(TResult));

        public static T With<T>(this T target, params Action<T>[] actions)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (actions == null || actions.Length == 0)
                throw new ArgumentNullException(nameof(actions));

            foreach (var a in actions)
                a(target);

            return target;
        }

        public static TOut Match<T, T1, T2, TOut>(this T source, Func<T1, TOut> f1, Func<T2, TOut> f2, Func<TOut> fDefault = null) where T1 : T where T2 : T
        {
            if (source is T1)
                return f1((T1)source);
            else if (source is T2)
                return f2((T2)source);
            else if (fDefault != null)
                return fDefault();
            return default(TOut);
        }
    }
}