using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Mimp.SeeSharper.Instantiation.Abstraction
{
    [Serializable]
    public class InstantiationException : Exception
    {

        public Type Type { get; }

        public object? InstantiateValues { get; }

        public string? MemberPath { get; }


        public string SingleMessage => base.Message;

        private string? _message;
        public override string Message
        {
            get
            {
                if (_message is not null)
                    return _message;
                IEnumerable<string> GetSubMessages(string? memberPath, Exception? inner)
                {
                    if (inner is null)
                        yield break;
                    var path = memberPath ?? "";
                    var lastMessage = "";
                    var message = "";
                    while (inner is not null)
                    {
                        if (inner is InstantiationException iex)
                        {
                            path += iex.MemberPath;
                            inner = inner.InnerException;
                            lastMessage = message;
                            message = iex.SingleMessage;
                        }
                        else if (inner is AggregateException aex)
                        {
                            if (aex.InnerExceptions.Count > 1)
                                //(string)typeof(AggregateException)
                                //    .GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(aex)
                                foreach (var m in Regex.Split(message, @"\r\n|\r|\n"))
                                    yield return m;
                            foreach (var ex in aex.InnerExceptions)
                                foreach (var sm in GetSubMessages(path, ex))
                                    foreach (var m in Regex.Split(sm, @"\r\n|\r|\n"))
                                        yield return $"{(aex.InnerExceptions.Count > 1 ? "\t" : "")}{m}";
                            yield break;
                        }
                        else
                        {
                            lastMessage = message;
                            message = inner.Message;
                            break;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(path))
                        lastMessage = $@"Can't create member ""{Type.Name}{path}""{(string.IsNullOrWhiteSpace(lastMessage) ? "." : $": {lastMessage}")}";
                    if (!string.IsNullOrWhiteSpace(lastMessage))
                        foreach (var m in Regex.Split(lastMessage, @"\r\n|\r|\n"))
                            yield return $"{m}";
                    foreach (var m in Regex.Split(message, @"\r\n|\r|\n"))
                        yield return $"\t{m}";
                }
                _message = SingleMessage;
                var subMsg = string.Join(Environment.NewLine, GetSubMessages(MemberPath, InnerException).Select(m => $"\t{m}"));
                if (!string.IsNullOrWhiteSpace(subMsg))
                    _message += $"{Environment.NewLine}{subMsg}";
                return _message;
            }
        }


        public InstantiationException(Type type, object? instantiateValues)
            : this(type, instantiateValues, null) { }

        public InstantiationException(Type type, object? instantiateValues, string? memberPath)
            : this(type, instantiateValues, memberPath, null, (Exception?)null) { }

        public InstantiationException(Type type, object? instantiateValues, string? memberPath, string? message)
            : this(type, instantiateValues, memberPath, message, (Exception?)null) { }

        public InstantiationException(Type type, object? instantiateValues, string? memberPath, Exception? inner)
            : this(type, instantiateValues, memberPath, null, inner) { }

        public InstantiationException(Type type, object? instantiateValues, string? memberPath, IEnumerable<Exception> inners)
            : this(type, instantiateValues, memberPath, null, inners) { }

        public InstantiationException(Type type, object? instantiateValues, string? memberPath, string? message, IEnumerable<Exception> inners)
            : this(type, instantiateValues, memberPath, message, new AggregateException(message, inners)) { }

        public InstantiationException(Type type, object? instantiateValues, string? memberPath, string? message, Exception? inner)
            : base(message, inner)
        {
            Type = type;
            InstantiateValues = instantiateValues;
            MemberPath = memberPath;
        }


        protected InstantiationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            if (Type is null)
                Type = typeof(void);
        }


        public static InstantiationException GetNotMatchingTypeException(IInstantiator instantiator, Type type) =>
            new InstantiationException(type, null, null, $@"{instantiator} can't handle type ""{type}"".");


        public static InstantiationException GetNoMemberException(Type type, object? instantiateValues, string? name) =>
            new InstantiationException(type, instantiateValues, $".{name}", $@"{type} has no member ""{name}"".");


        public static InstantiationException GetCanNotInstantiateParameterException(Type type, object? instantiateValues, ConstructorInfo constructor, ParameterInfo parameter, IEnumerable<Exception> inners) =>
            GetCanNotInstantiateParameterException(type, instantiateValues, constructor, parameter, new AggregateException(GetCantInstantiateParameterMessage(constructor, parameter), inners));

        public static InstantiationException GetCanNotInstantiateParameterException(Type type, object? instantiateValues, ConstructorInfo constructor, ParameterInfo parameter, Exception? inner) =>
            new InstantiationException(type, instantiateValues, null, GetCantInstantiateParameterMessage(constructor, parameter), inner);

        public static InstantiationException GetCanNotInstantiateParameterException(Type type, object? instantiateValues, ConstructorInfo constructor, ParameterInfo parameter) =>
            GetCanNotInstantiateParameterException(type, instantiateValues, constructor, parameter, (Exception?)null);


        public static InstantiationException GetCanNotInstantiateExeption(Type type, object? instantiateValues, string? memberPath, Exception? inner) =>
            new InstantiationException(type, instantiateValues, memberPath, GetCantInstantiateMessage(type, instantiateValues), inner);

        public static InstantiationException GetCanNotInstantiateExeption(Type type, object? instantiateValues, Exception? inner) =>
            GetCanNotInstantiateExeption(type, instantiateValues, null, inner);

        public static InstantiationException GetCanNotInstantiateExeption(Type type, object? instantiateValues, string? memberPath, IEnumerable<Exception> inners) =>
            GetCanNotInstantiateExeption(type, instantiateValues, memberPath, new AggregateException(GetCantInstantiateMessage(type, instantiateValues), inners));

        public static InstantiationException GetCanNotInstantiateExeption(Type type, object? instantiateValues, IEnumerable<Exception> inners) =>
            GetCanNotInstantiateExeption(type, instantiateValues, null, inners);

        public static InstantiationException GetCanNotInstantiateExeption(Type type, object? instantiateValues, string? memberPath) =>
            GetCanNotInstantiateExeption(type, instantiateValues, memberPath, (Exception?)null);

        public static InstantiationException GetCanNotInstantiateExeption(Type type, object? instantiateValues) =>
            GetCanNotInstantiateExeption(type, instantiateValues, (Exception?)null);


        public static string GetCantInstantiateParameterMessage(ConstructorInfo constructor, ParameterInfo parameter) =>
            $@"Can't create value for parameter ""{parameter.Name}"" of ""{constructor}"".";

        public static string GetCantInstantiateMessage(Type type, object? instantiateValues) =>
            $@"Can't create ""{type}"" from value ""{instantiateValues}"".";


    }
}
