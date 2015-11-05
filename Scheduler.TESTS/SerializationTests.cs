using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Scheduler.TESTS
{
    [Trait("Category", "Serialization")]
    public class SerializationTests
    {
        [Fact(DisplayName = "SerializeTimespan")]
        public void SerializeTimespan()
        {
            var time = DateTime.Now.TimeOfDay;
            var model = new Model
            {
                Name = "Testing Hours",
                Interval = time
            };
            var val = JsonConvert.SerializeObject(model);
            Assert.NotNull(val);
            var x = JsonConvert.DeserializeObject<Model>(val);
            Assert.Equal(time, x.Interval);
        }

        public class Model
        {
            public string Name { get; set; }
            public TimeSpan Interval { get; set; }
        }
    }
}
