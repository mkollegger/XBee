﻿using System;
using System.Threading.Tasks;
using XBee.Frames;
using XBee.Frames.AtCommands;

namespace XBee.Devices
{
    internal class XBeePro900HP : XBeeNode
    {
        internal XBeePro900HP(XBeeController controller, 
            HardwareVersion hardwareVersion = HardwareVersion.XBeePro900HP,
            NodeAddress address = null) : base(controller, hardwareVersion, address)
        {
        }

        public override async Task<NodeAddress> GetAddress()
        {
            var high = await ExecuteAtQueryAsync<PrimitiveResponseData<uint>>(new DestinationAddressHighCommand());
            var low = await ExecuteAtQueryAsync<PrimitiveResponseData<uint>>(new DestinationAddressLowCommand());

            var address = new LongAddress(high.Value, low.Value);

            return new NodeAddress(address);
        }

        public async Task<NodeMessagingOptions> GetNodeMessagingOptions()
        {
            CoordinatorEnableResponseData response = 
                await ExecuteAtQueryAsync<CoordinatorEnableResponseData>(new CoordinatorEnableCommandExt());

            if(response.Options == null)
                throw new InvalidOperationException("No valid coordinator state returned.");

            return response.Options.Value;
        }

        public async Task SetNodeMessagingOptions(NodeMessagingOptions options)
        {
            await ExecuteAtCommandAsync(new CoordinatorEnableCommandExt(options));
        }

        public override async Task SetChangeDetectionChannels(DigitalSampleChannels channels)
        {
            await ExecuteAtCommandAsync(new InputOutputChangeDetectionCommandExt(channels));
        }

        public override async Task TransmitDataAsync(byte[] data)
        {
            if (Address == null)
                throw new InvalidOperationException("Can't send data to local device.");

            var transmitRequest = new TxRequestExtFrame(Address.LongAddress, data);
            TxStatusExtFrame response = await Controller.ExecuteQueryAsync<TxStatusExtFrame>(transmitRequest);

            if (response.DeliveryStatus != DeliveryStatusExt.Success)
                throw new XBeeException(string.Format("Delivery failed with status code '{0}'.",
                    response.DeliveryStatus));
        }

        public async Task<SleepOptionsExt> GetSleepOptions()
        {
            var response = await ExecuteAtQueryAsync<SleepOptionsResponseData>(new SleepOptionsCommand());

            if (response.OptionsExt == null)
                throw new InvalidOperationException("No valid sleep options returned.");

            return response.OptionsExt.Value;
        }

        public async Task SetSleepOptions(SleepOptionsExt options)
        {
            await ExecuteAtCommandAsync(new SleepOptionsCommandExt(options));
        }
    }
}
