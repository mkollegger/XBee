﻿using System;
using System.Threading.Tasks;
using XBee.Frames;
using XBee.Frames.AtCommands;

namespace XBee
{
    public class XBeeNode
    {
        private readonly XBeeController _controller;

        internal XBeeNode(XBeeController controller, NodeAddress address = null)
        {
            _controller = controller;
            Address = address;
        }

        public NodeAddress Address { get; set; }

        public async Task<string> GetNodeIdentifier()
        {
            NodeIdentifierResponseData response =
                await ExecuteAtQueryAsync<NodeIdentifierResponseData>(new NodeIdentifierCommand());
            return response.Id;
        }

        public async Task SetNodeIdentifier(string id)
        {
            await ExecuteAtCommandAsync(new NodeIdentifierCommand(id));
        }

        public async Task<LongAddress> GetSerialNumber()
        {
            PrimitiveResponseData<uint> highAddress =
                await ExecuteAtQueryAsync<PrimitiveResponseData<UInt32>>(new SerialNumberHighCommand());
            PrimitiveResponseData<uint> lowAddress =
                await ExecuteAtQueryAsync<PrimitiveResponseData<UInt32>>(new SerialNumberLowCommand());

            return new LongAddress(highAddress.Value, lowAddress.Value);
        }

        public async Task<bool> IsCoordinator()
        {
            CoordinatorEnableResponseData response;
            if (_controller.ControllerHardwareVersion == HardwareVersion.XBeePro900HP)
                response = await ExecuteAtQueryAsync<CoordinatorEnableResponseData>(new CoordinatorEnableCommandExt());
            else response = await ExecuteAtQueryAsync<CoordinatorEnableResponseData>(new CoordinatorEnableCommand());

            if (response.EnableState != null)
                return response.EnableState.Value == CoordinatorEnableState.Coordinator;

            if (response.EnableStateExt != null)
                return response.EnableStateExt.Value == CoordinatorEnableStateExt.NonRoutingCoordinator;

            throw new InvalidOperationException("No coordinator state returned.");
        }

        public async Task SetCoordinator(bool enable)
        {
            if (_controller.ControllerHardwareVersion == HardwareVersion.XBeePro900HP)
                await ExecuteAtCommandAsync(new CoordinatorEnableCommandExt(enable));
            else await ExecuteAtCommandAsync(new CoordinatorEnableCommand(enable));
        }

        public async Task<InputOutputConfiguration> GetInputOutputConfiguration(InputOutputChannel channel)
        {
            InputOutputResponseData response =
                await ExecuteAtQueryAsync<InputOutputResponseData>(new InputOutputConfigurationCommand(channel));
            return response.Value;
        }

        public async Task SetInputOutputConfiguration(InputOutputChannel channel, InputOutputConfiguration configuration)
        {
            if (_controller.ControllerHardwareVersion == HardwareVersion.XBeePro900 ||
                _controller.ControllerHardwareVersion == HardwareVersion.XBeePro900HP)
                await ExecuteAtCommandAsync(new InputOutputConfigurationCommandExt(channel, configuration));
            else await ExecuteAtCommandAsync(new InputOutputConfigurationCommand(channel, configuration));
        }

        public async Task<DigitalSampleChannels> GetChangeDetection()
        {
            PrimitiveResponseData<DigitalSampleChannels> response =
                await ExecuteAtQueryAsync<PrimitiveResponseData<DigitalSampleChannels>>(
                    new InputOutputChangeDetectionCommand());
            return response.Value;
        }

        public async Task SetChangeDetection(DigitalSampleChannels channels)
        {
            await ExecuteAtCommandAsync(new InputOutputChangeDetectionCommand(channels));
        }

        public async Task ForceSample()
        {
            await ExecuteAtCommandAsync(new ForceSampleCommand());
        }

        public async Task<TimeSpan> GetSampleRate()
        {
            var response = await ExecuteAtQueryAsync<SampleRateResponseData>(new SampleRateCommand());
            return response.Interval;
        }

        public async Task SetSampleRate(TimeSpan interval)
        {
            await ExecuteAtCommandAsync(new SampleRateCommand(interval));
        }

        public async Task<bool> IsEncryptionEnabled()
        {
            var response = await ExecuteAtQueryAsync<PrimitiveResponseData<bool>>(new EncryptionEnableCommand());
            return response.Value;
        }

        public async Task SetEncryptionEnabled(bool enabled)
        {
            await ExecuteAtCommandAsync(new EncryptionEnableCommand(enabled));
        }

        public async Task SetEncryptionKey(byte[] key)
        {
            await ExecuteAtCommandAsync(new EncryptionKeyCommand(key));
        }

        public async Task WriteChanges()
        {
            await ExecuteAtCommandAsync(new WriteCommand());
        }

        private async Task<TResponseData> ExecuteAtQueryAsync<TResponseData>(AtCommand command)
            where TResponseData : AtCommandResponseFrameData
        {
            return await _controller.ExecuteAtQueryAsync<TResponseData>(command, Address);
        }

        private async Task ExecuteAtCommandAsync(AtCommand command)
        {
            await _controller.ExecuteAtCommandAsync(command, Address);
        }
    }
}