using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpComparer {
    public class Comparator {
        private readonly Dictionary<string, object> _left;
        private readonly Dictionary<string, object> _right;

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
                if(_right.ContainsKey(propName)) {
                    var leftValue = _left[propName];
                    var rightValue = _right[propName];

                    if (!(leftValue == null && rightValue == null) &&
                        (!leftValue?.Equals(rightValue) ?? true)) {
                        result.AddError(propName, ErrorType.NotEqual);
                    }
                    _left.Remove(propName);
                    _right.Remove(propName);
                }
                else {
                    result.AddError(propName, ErrorType.MissingOnRight);
                }
            }
            var rightProps = _right.Keys.ToList();
            foreach (var rightPropName in rightProps) {
                result.AddError(rightPropName, ErrorType.MissingOnLeft);
            }
            return result;
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
    }

    public class CompareResult {
        public bool Success => Errors.Count == 0;
        public List<CompareError> Errors { get; private set; } = new List<CompareError>();

        public void AddError(string prop, ErrorType error) {
            Errors.Add(new CompareError { Property = prop, Error = error });
        }

        public string GetAllErrors() {
            var sb = new StringBuilder();
            foreach (var e in Errors) {
                sb.AppendLine($"Property [{e.Property}] Error: {e.Error}");
            }
            return sb.ToString();
        }
    }

    public class CompareError {
        public string Property { get; set; }
        public ErrorType Error { get; set; }
    }

    public enum ErrorType {
        NotEqual,
        MissingOnRight,
        MissingOnLeft
    }
}
