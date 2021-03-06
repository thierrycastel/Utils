﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Interface;

namespace Utils
{
    public static class Optional
    {
        public static IOptional<T> Of<T>(Func<T> func)
            => Some(func());

        public static IOptional<T> Some<T>(T value)
            => value == null
                ? Optional<T>.None()
                : Optional<T>.Create(value);
    }
    
    /// <summary>
    /// Implements the optional pattern
    /// </summary>
    /// <typeparam name="T">The type of the optional instance</typeparam>
    public sealed class Optional<T> : IOptional<T>
    {
        private static readonly IOptional<T> _noneInstance = CreateNone();
        private static Action VoidAction { get; } = () => { /* do nothing */ };

        public static IOptional<T> None()
            => _noneInstance;

        private List<T> Values { get; } = new List<T>();
        private Action ExecuteSome { get; set; } = VoidAction;
        private Func<T> ExecuteNone { get; set; } = () => default(T);

        private Optional()
        {
            // nothing to do (Values property is already set to an empty list)
        }

        /// <summary>
        /// private constructor
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private Optional(T value)
        {
            //if (value == null) { throw new ArgumentNullException(nameof(value)); }
            //if (value.Count() > 1) { throw new ArgumentException($"Collection '{nameof(value)}' contains more than one element"); }

            Values = new List<T>{value};
        }

        private static Optional<T> CreateNone()
            => new Optional<T>(); 
        internal static Optional<T> Create(T value)
            => value == null
                ? CreateNone()
                : new Optional<T>(value);
        
        public IOptional<T> WhenSome(Action action)
        {
            ExecuteSome = action ?? throw new ArgumentNullException(nameof(action));
            
            if (Values.Any())
            {
                ExecuteSome.Invoke();
            }
            return this;
        }

        public IOptional<T> WhenNone(Func<T> func)
        {
            ExecuteNone = func ?? throw new ArgumentNullException(nameof(func));
            
            if (!Values.Any())
            {
                ExecuteNone();
            }
            return this;
        }

        public IOptional<T> WhenSome(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return Values.Select(v => WhenSome(v, predicate))
                         .DefaultIfEmpty(None())
                         .Single();
            
        }
        private IOptional<T> WhenSome(T value, Func<T, bool> predicate)
            => predicate == null || predicate(value)
                ? Create(value)
                : None();

        public T Map()
            => Values.Any()
                ? Values.First()
                : ExecuteNone();
    }
}