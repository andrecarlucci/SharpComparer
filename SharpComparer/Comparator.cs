using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpComparer {
    public class Comparator {
        private readonly Dictionary<string, object> _left;
        private readonly Dictionary<string, object> _right;
        private bool _useSizeToCompareCollections;

        public static Comparator With(object left, object right) {
            return new Comparator(left, right);
        }

        private Comparator(object left, object right) {
            _left = ToDictionary(left);
            _right = ToDictionary(right);
        }

        public CompareResult Compare() {
            var result = new CompareResult();

            var leftProps = _left.Keys.ToList();
            foreach(var propName in leftProps) {
                var leftValue = _left[propName];
                if (_right.ContainsKey(propName)) {
                    var rightValue = _right[propName];

                    if(!BothAreNull(leftValue, rightValue)) {
                        if(!CompareViaCollectionSize(propName, leftValue, rightValue)) {
                            CompareViaEqual(propName, leftValue, rightValue);
                        }
                    }
                }
                else {
                    result.AddError(propName, ErrorType.MissingOnRight, leftValue, null);
                }
                _left.Remove(propName);
                _right.Remove(propName);
            }
            var rightProps = _right.Keys.ToList();
            foreach (var rightPropName in rightProps) {
                var rightValue = _right[rightPropName];
                result.AddError(rightPropName, ErrorType.MissingOnLeft, null, rightValue);
            }
            return result;

            bool BothAreNull(object left, object right) {
                return left == null && right == null;
            }

            bool CompareViaCollectionSize(string propertyName, object left, object right) {
                if(_useSizeToCompareCollections &&
                   left is ICollection cLeft &&
                   right is ICollection cRight) {

                    if(cLeft.Count != cRight.Count) {
                        result.AddError(propertyName, ErrorType.NotEqual, cLeft.Count, cRight.Count);
                    }
                    return true;
                }
                return false;
            }

            void CompareViaEqual(string propertyName, object left, object right) {
                if (!left?.Equals(right) ?? true) {
                    result.AddError(propertyName, ErrorType.NotEqual, left, right);
                }
            }
        }

        private Dictionary<string, object> ToDictionary(object obj) {
            var dictionary = new Dictionary<string, object>();
            foreach (var property in obj.GetType().GetProperties()) { 
                dictionary.Add(property.Name, property.GetValue(obj));
            }
            return dictionary;
        }

        public Comparator IgnoreLeft(params string[] propertyNames) {
            if(propertyNames == null) {
                return this;
            }
            foreach(var p in propertyNames) {
                _left.Remove(p);
            }
            return this;
        }

        public Comparator IgnoreRight(params string[] propertyNames) {
            if (propertyNames == null) {
                return this;
            }
            foreach (var p in propertyNames) {
                _right.Remove(p);
            }
            return this;
        }

        public Comparator Ignore(params string[] propertyNames) {
            IgnoreLeft(propertyNames);
            IgnoreRight(propertyNames);
            return this;
        }

        public Comparator EnhanceLeft(string propertyName, object value) {
            _left.Add(propertyName, value);
            return this;
        }
        public Comparator EnhanceRight(string propertyName, object value) {
            _right.Add(propertyName, value);
            return this;
        }

        public Comparator ReplaceLeft(string propertyName, object value) {
            _left[propertyName] = value;
            return this;
        }
        public Comparator ReplaceRight(string propertyName, object value) {
            _right[propertyName] = value;
            return this;
        }

        public Comparator UseSizeToCompareCollections() {
            _useSizeToCompareCollections = true;
            return this;
        }
    }

    public class CompareResult {
        public bool Success => Errors.Count == 0;
        public List<CompareError> Errors { get; private set; } = new List<CompareError>();

        public void AddError(string prop, ErrorType error, object left, object right) {
            Errors.Add(new CompareError(prop, error, left, right)); 
        }

        public string GetAllErrors() {
            var sb = new StringBuilder();
            foreach (var e in Errors) {
                sb.AppendLine(e.Msg);
            }
            return sb.ToString();
        }
    }

    public class CompareError {
        public string Property { get; set; }
        public ErrorType Error { get; set; }

        public string Msg { get; private set; }

        public CompareError(string property, ErrorType errorType, object left, object right) {
            Property = property;
            Error = errorType;
            Msg = $"Property [{property}] Error: [{errorType}] Left: [{left ?? "<NULL>"} Right: [{right ?? "<NULL>"}]";
        }
    }

    public enum ErrorType {
        NotEqual,
        MissingOnRight,
        MissingOnLeft
    }
}
