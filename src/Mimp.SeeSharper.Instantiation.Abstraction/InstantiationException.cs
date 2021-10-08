using Mimp.SeeSharper.ObjectDescription.Abstraction;
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

        public IObjectDescription? Description { get; }

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
                                foreach (var m in Regex.Split(message, @"
|\r|\n"))
                                    yield return m;
                            foreach (var ex in aex.InnerExceptions)
                                foreach (var sm in GetSubMessages(path, ex))
                                    foreach (var m in Regex.Split(sm, @"
|\r|\n"))
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
                        foreach (var m in Regex.Split(lastMessage, @"
|\r|\n"))
                            yield return $"{m}";
                    foreach (var m in Regex.Split(message, @"
|\r|\n"))
                        yield return $"\t{m}";
                }
                _message = SingleMessage;
                var subMsg = string.Join(Environment.NewLine, GetSubMessages(MemberPath, InnerException).Select(m => $"\t{m}"));
                if (!string.IsNullOrWhiteSpace(subMsg))
                    _message += $"{Environment.NewLine}{subMsg}";
                return _message;
            }
        }


        public InstantiationException(Type type, IObjectDescription description)
            : this(type, description, null) { }

        public InstantiationException(Type type, IObjectDescription description, string? memberPath)
            : this(type, description, memberPath, null, (Exception?)null) { }

        public InstantiationException(Type type, IObjectDescription description, string? memberPath, string? message)
            : this(type, description, memberPath, message, (Exception?)null) { }

        public InstantiationException(Type type, IObjectDescription description, string? memberPath, Exception? inner)
            : this(type, description, memberPath, null, inner) { }

        public InstantiationException(Type type, IObjectDescription description, string? memberPath, IEnumerable<Exception> inners)
            : this(type, description, memberPath, null, inners) { }

        public InstantiationException(Type type, IObjectDescription description, string? memberPath, string? message, IEnumerable<Exception> inners)
            : this(type, description, memberPath, message, new AggregateException(message, inners)) { }

        public InstantiationException(Type type, IObjectDescription description, string? memberPath, string? message, Exception? inner)
            : base(message, inner)
        {
            Type = type;
            Description = description;
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


        public static InstantiationException GetNotMatchingTypeException(IInstantiator instantiator, Type type, IObjectDescription description) =>
            new InstantiationException(type, description, null, $@"{instantiator} can't handle type ""{type}"".");


        public static InstantiationException GetNoMemberException(Type type, IObjectDescription description, string? name) =>
            new InstantiationException(type, description, $".{name}", $@"{type} has no member ""{name}"".");


        public static InstantiationException GetCanNotInstantiateParameterException(Type type, IObjectDescription description, ConstructorInfo constructor, ParameterInfo parameter, IEnumerable<Exception> inners) =>
            GetCanNotInstantiateParameterException(type, description, constructor, parameter, new AggregateException(GetCanNotInstantiateParameterMessage(constructor, parameter), inners));

        public static InstantiationException GetCanNotInstantiateParameterException(Type type, IObjectDescription description, ConstructorInfo constructor, ParameterInfo parameter, Exception? inner) =>
            new InstantiationException(type, description, $"({parameter.Name})", GetCanNotInstantiateParameterMessage(constructor, parameter), inner);

        public static InstantiationException GetCanNotInstantiateParameterException(Type type, IObjectDescription description, ConstructorInfo constructor, ParameterInfo parameter) =>
            GetCanNotInstantiateParameterException(type, description, constructor, parameter, (Exception?)null);


        public static InstantiationException GetCanNotInstantiateException(Type type, IObjectDescription description, string? memberPath, Exception? inner) =>
            new InstantiationException(type, description, memberPath, GetCanNotInstantiateMessage(type, description), inner);

        public static InstantiationException GetCanNotInstantiateException(Type type, IObjectDescription description, Exception? inner) =>
            GetCanNotInstantiateException(type, description, null, inner);

        public static InstantiationException GetCanNotInstantiateException(Type type, IObjectDescription description, string? memberPath, IEnumerable<Exception> inners) =>
            GetCanNotInstantiateException(type, description, memberPath, new AggregateException(GetCanNotInstantiateMessage(type, description), inners));

        public static InstantiationException GetCanNotInstantiateException(Type type, IObjectDescription description, IEnumerable<Exception> inners) =>
            GetCanNotInstantiateException(type, description, null, inners);

        public static InstantiationException GetCanNotInstantiateException(Type type, IObjectDescription description, string? memberPath) =>
            GetCanNotInstantiateException(type, description, memberPath, (Exception?)null);

        public static InstantiationException GetCanNotInstantiateException(Type type, IObjectDescription description) =>
            GetCanNotInstantiateException(type, description, (Exception?)null);


        public static InstantiationException GetUsedNotAllException(Type type, IObjectDescription description, IObjectDescription ignored) =>
            new InstantiationException(type, description, GetUsedNotAllMessage(type, description, ignored));


        public static string GetCanNotInstantiateParameterMessage(ConstructorInfo constructor, ParameterInfo parameter) =>
            $@"Can't create value for parameter ""{parameter.Name}"" of ""{constructor}"".";

        public static string GetCanNotInstantiateMessage(Type type, IObjectDescription description) =>
            $@"Can't create ""{type}"" from ""{description}"".";

        public static string GetUsedNotAllMessage(Type type, IObjectDescription description, IObjectDescription ignored) =>
            $@"Used not all ""{description}"" for ""{type}"": {ignored}";


    }
}
