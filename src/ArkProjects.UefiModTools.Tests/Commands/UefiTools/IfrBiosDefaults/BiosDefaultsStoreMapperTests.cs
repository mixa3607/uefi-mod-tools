using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrBiosDefaults;

public class BiosDefaultsStoreMapperTests
{
    [Fact]
    public void MapCreatesVersionedEmptyStoreMap()
    {
        var biosDefaultsMap = CreateBiosDefaultsMap();
        var ifr = new IfrJsonDocument
        {
            InputSha256 = "ifr-sha256",
            Operations = [new IfrOperation { Opcode = "VarStore" }],
        };

        var result = CreateMapper().Map(biosDefaultsMap, ifr);

        Assert.Equal(BiosDefaultsStoreMapDocument.SupportedVersion, result.Version);
        Assert.Equal(BiosDefaultsStoreMapDocument.SupportedType, result.Type);
        Assert.Equal("defaults-sha256", result.BiosDefaultsSha256);
        Assert.Equal("ifr-sha256", result.IfrSha256);
        Assert.Empty(result.QuestionMappings);
    }

    [Fact]
    public void MapRejectsUnsupportedBiosDefaultsMap()
    {
        var biosDefaultsMap = CreateBiosDefaultsMap();
        biosDefaultsMap.Version = 2;

        var error = Assert.Throws<ArgumentException>(() => CreateMapper().Map(biosDefaultsMap, new IfrJsonDocument()));

        Assert.Equal("Expected AF516361-BiosDefaults-Map version 1 (Parameter 'biosDefaultsMap')", error.Message);
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
        var ifr = new IfrJsonDocument
        {
            Operations =
            [
                new IfrOperation
                {
                    Opcode = "VarStore",
                    Fields = new IfrOperationFields
                    {
                        VarStoreId = 1,
                        Name = JsonSerializer.SerializeToElement("Setup"),
                        Size = 24,
                    },
                },
                new IfrOperation
                {
                    Opcode = "Numeric",
                    Fields = new IfrOperationFields
                    {
                        QuestionId = 7,
                        VarStoreId = 1,
                        VarOffset = 2,
                        MinMaxStep = new IfrMinMaxStep { SizeBits = 16 },
                    },
                },
            ],
        };

        var mapping = Assert.Single(CreateMapper().Map(biosDefaultsMap, ifr).QuestionMappings);

        Assert.Equal((ushort)7, mapping.QuestionId);
        Assert.Equal("Numeric", mapping.Opcode);
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
