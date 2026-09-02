using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrBiosDefaults;

public class BiosDefaultsStoreMapperTests
{
    [Fact]
    public void MapReturnsNoMappingsWithoutStorageQuestions()
    {
        var mappings = CreateMapper().Map([], [new IfrOperation { Opcode = IfrOpCodes.VarStore }]);

        Assert.Empty(mappings);
    }

    [Fact]
    public void MapMatchesQuestionToNvarVariable()
    {
        var biosDefaultsMap = CreateBiosDefaultsMap();
        biosDefaultsMap.Variables =
        [
            new NvarVariableInfo
            {
                Name = "Setup",
                RecordOffset = 0,
                RecordSize = 32,
                DataOffset = 8,
            },
        ];
        IfrOperation[] ifrOperations =
        [
            new IfrOperation
            {
                Opcode = IfrOpCodes.VarStore,
                Fields = new IfrOperationFields
                {
                    VarStoreId = 1,
                    Name = JsonSerializer.SerializeToElement("Setup"),
                    Size = 24,
                },
            },
            new IfrOperation
            {
                Opcode = IfrOpCodes.Numeric,
                Fields = new IfrOperationFields
                {
                    QuestionId = 7,
                    VarStoreId = 1,
                    VarOffset = 2,
                    MinMaxStep = new IfrMinMaxStep { SizeBits = 16 },
                },
            },
        ];

        var mapping = Assert.Single(CreateMapper().Map(biosDefaultsMap.Variables, ifrOperations));

        Assert.Equal((ushort)7, mapping.QuestionId);
        Assert.Equal(IfrOpCodes.Numeric, mapping.Opcode);
        Assert.Equal("Setup", mapping.VarStoreName);
        Assert.Equal((ushort)2, mapping.VarStoreOffset);
        Assert.Equal(2, mapping.DataLength);
        Assert.Equal(BiosDefaultsMappingStatus.Mapped, mapping.Status);
        Assert.Equal(10, mapping.NvarDataOffset);
    }

    private static BiosDefaultsStoreMapper CreateMapper() => new(NullLogger<BiosDefaultsStoreMapper>.Instance);

    private static BiosDefaultsMapDocument CreateBiosDefaultsMap() => new()
    {
        Version = BiosDefaultsMapDocument.SupportedVersion,
        Type = BiosDefaultsMapDocument.SupportedType,
        SourceName = "AF516361-BiosDefaults.bin",
        SourceSha256 = "defaults-sha256",
        Variables = [],
    };
}
