using Bond;
using Bond.IO.Safe;
using Bond.Protocols;
using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoyaStore.Data
{
    public static class ModelSerializer
    {
        public static T DeserializeModel<T>(byte[] data)
        {
            var input = new InputBuffer(data);
            var reader = new FastBinaryReader<InputBuffer>(input);

            return Deserialize<T>.From(reader);
        }

        public static byte[] SerializeModel<T>(T model)
        {
            var output = new OutputBuffer();
            var writer = new FastBinaryWriter<OutputBuffer>(output);

            // The first calls to Serialize.To and Deserialize<T>.From can take
            // a relatively long time because they generate the de/serializer
            // for a given type and protocol.
            Serialize.To(writer, model);
            return output.Data.Array;
        }
    }
}
