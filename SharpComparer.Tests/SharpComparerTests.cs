using Xunit;

namespace SharpComparer.Tests {
    public class SharpComparerTests {

        [Fact]
        public void Should_compare_props() {
            var foo = new Foo { Name = "n", Age = 1 };
            var fooClone = new FooClone { Name = "n", Age = 1 };
            var result = Comparator.With(foo, fooClone)
                                      .Compare();

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_compare_null() {
            var foo = new Foo { Name = null, Age = 1 };
            var fooClone = new FooClone { Name = null, Age = 1 };
            var result = Comparator.With(foo, fooClone)
                                      .Compare();
            Assert.True(result.Success);
        }

        [Fact]
        public void Should_compare_null_on_left() {
            var left = new Foo { Name = null, Age = 1 };
            var right = new FooClone { Name = "n", Age = 1 };
            var result = Comparator.With(left, right)
                                      .Compare();
            Assert.False(result.Success);
            Assert.Equal(nameof(left.Name), result.Errors[0].Property);
            Assert.Equal(ErrorType.NotEqual, result.Errors[0].Error);
        }

        [Fact]
        public void Should_return_errors() {
            var foo = new Foo { Name = "n", Age = 1 };
            var fooClone = new FooClone { Name = "n", Age = 2 };
            var result = Comparator.With(foo, fooClone)
                                      .Compare();

            Assert.False(result.Success);
            Assert.Equal(nameof(foo.Age), result.Errors[0].Property);
            Assert.Equal(ErrorType.NotEqual, result.Errors[0].Error);
        }

        [Fact]
        public void Should_verify_missing_props_on_left() {
            var foo = new Foo { Name = "n", Age = 1 };
            var bar = new Bar { Name = "n", Age = 1 };
            var result = Comparator.With(foo, bar)
                                      .Compare();

            Assert.False(result.Success);
            Assert.Equal(nameof(bar.Address), result.Errors[0].Property);
            Assert.Equal(ErrorType.MissingOnLeft, result.Errors[0].Error);
        }

        [Fact]
        public void Should_verify_missing_props_on_right() {
            var foo = new Foo { Name = "n", Age = 1 };
            var bar = new Bar { Name = "n", Age = 1 };
            var result = Comparator.With(bar, foo)
                                      .Compare();

            Assert.False(result.Success);
            Assert.Equal(nameof(bar.Address), result.Errors[0].Property);
            Assert.Equal(ErrorType.MissingOnRight, result.Errors[0].Error);
        }

        [Fact]
        public void Should_add_left_exceptions() {
            var l = new Bar { Name = "n", Age = 1 };
            var r = new Foo { Name = "n", Age = 1 };
            var result = Comparator.With(l, r)
                                   .IgnoreLeft(nameof(l.Address))
                                   .Compare();

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_add_right_exceptions() {
            var l = new Foo { Name = "n", Age = 1 };
            var r = new Bar { Name = "n", Age = 1 };
            var result = Comparator.With(l, r)
                                   .IgnoreRight(nameof(r.Address))
                                   .Compare();

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_add_exceptions() {
            var l = new Foo { Name = "n", Age = 12 };
            var r = new Foo { Name = "n", Age = 1 };
            var result = Comparator.With(l, r)
                                   .Ignore(nameof(r.Age))
                                   .Compare();

            Assert.True(result.Success);
        }

        [Fact]
        public void Should_enhance_left() {
            var l = new Foo { Name = "n", Age = 1 };
            var r = new Bar { Name = "n", Age = 1, Address = "a" };
            var result = Comparator.With(l, r)
                                   .EnhanceLeft(nameof(r.Address), "a")
                                   .Compare();

            Assert.True(result.Success, result.GetAllErrors());
        }

        [Fact]
        public void Should_enhance_right() {
            var l = new Foo { Name = "n", Age = 1 };
            var r = new Bar { Name = "n", Age = 1, Address = "a" };
            var result = Comparator.With(r, l)
                                   .EnhanceRight(nameof(r.Address), "a")
                                   .Compare();

            Assert.True(result.Success, result.GetAllErrors());
        }

        public class Foo {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        
        public class FooClone {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class Bar {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Address { get; set; }
        }
    }
}
