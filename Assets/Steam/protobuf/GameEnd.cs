// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: game_end.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from game_end.proto</summary>
public static partial class GameEndReflection {

  #region Descriptor
  /// <summary>File descriptor for game_end.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static GameEndReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "Cg5nYW1lX2VuZC5wcm90byJrCgdHYW1lRW5kEg4KBndpbm5lchgBIAEoBBIi",
          "CgVzY29yZRgCIAMoCzITLkdhbWVFbmQuU2NvcmVFbnRyeRosCgpTY29yZUVu",
          "dHJ5EgsKA2tleRgBIAEoBBINCgV2YWx1ZRgCIAEoBToCOAFiBnByb3RvMw=="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::GameEnd), global::GameEnd.Parser, new[]{ "Winner", "Score" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
        }));
  }
  #endregion

}
#region Messages
[global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
public sealed partial class GameEnd : pb::IMessage<GameEnd>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<GameEnd> _parser = new pb::MessageParser<GameEnd>(() => new GameEnd());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pb::MessageParser<GameEnd> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::GameEndReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public GameEnd() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public GameEnd(GameEnd other) : this() {
    winner_ = other.winner_;
    score_ = other.score_.Clone();
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public GameEnd Clone() {
    return new GameEnd(this);
  }

  /// <summary>Field number for the "winner" field.</summary>
  public const int WinnerFieldNumber = 1;
  private ulong winner_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public ulong Winner {
    get { return winner_; }
    set {
      winner_ = value;
    }
  }

  /// <summary>Field number for the "score" field.</summary>
  public const int ScoreFieldNumber = 2;
  private static readonly pbc::MapField<ulong, int>.Codec _map_score_codec
      = new pbc::MapField<ulong, int>.Codec(pb::FieldCodec.ForUInt64(8, 0UL), pb::FieldCodec.ForInt32(16, 0), 18);
  private readonly pbc::MapField<ulong, int> score_ = new pbc::MapField<ulong, int>();
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public pbc::MapField<ulong, int> Score {
    get { return score_; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override bool Equals(object other) {
    return Equals(other as GameEnd);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool Equals(GameEnd other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Winner != other.Winner) return false;
    if (!Score.Equals(other.Score)) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override int GetHashCode() {
    int hash = 1;
    if (Winner != 0UL) hash ^= Winner.GetHashCode();
    hash ^= Score.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void WriteTo(pb::CodedOutputStream output) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    output.WriteRawMessage(this);
  #else
    if (Winner != 0UL) {
      output.WriteRawTag(8);
      output.WriteUInt64(Winner);
    }
    score_.WriteTo(output, _map_score_codec);
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
    if (Winner != 0UL) {
      output.WriteRawTag(8);
      output.WriteUInt64(Winner);
    }
    score_.WriteTo(ref output, _map_score_codec);
    if (_unknownFields != null) {
      _unknownFields.WriteTo(ref output);
    }
  }
  #endif

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public int CalculateSize() {
    int size = 0;
    if (Winner != 0UL) {
      size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Winner);
    }
    size += score_.CalculateSize(_map_score_codec);
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(GameEnd other) {
    if (other == null) {
      return;
    }
    if (other.Winner != 0UL) {
      Winner = other.Winner;
    }
    score_.MergeFrom(other.score_);
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(pb::CodedInputStream input) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    input.ReadRawMessage(this);
  #else
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 8: {
          Winner = input.ReadUInt64();
          break;
        }
        case 18: {
          score_.AddEntriesFrom(input, _map_score_codec);
          break;
        }
      }
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
          break;
        case 8: {
          Winner = input.ReadUInt64();
          break;
        }
        case 18: {
          score_.AddEntriesFrom(ref input, _map_score_codec);
          break;
        }
      }
    }
  }
  #endif

}

#endregion


#endregion Designer generated code
