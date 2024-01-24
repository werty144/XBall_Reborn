# Makefile to compile Protocol Buffers

PROTOC=protoc
SRC_DIR=./Assets/Steam/protobuf
DST_DIR=./Assets/Steam/protobuf

# Find all .proto files in the SRC_DIR
PROTO_FILES=$(wildcard $(SRC_DIR)/*.proto)

# Corresponding generated .cs files
GEN_FILES=$(patsubst $(SRC_DIR)/%.proto,$(DST_DIR)/%_pb.cs,$(PROTO_FILES))

# Default rule
proto: $(GEN_FILES)

# Rule for generating .cs files
$(DST_DIR)/%_pb.cs: $(SRC_DIR)/%.proto
	$(PROTOC) -I=$(SRC_DIR) --csharp_out=$(DST_DIR) $<

# Clean rule
clean:
	rm -f $(DST_DIR)/*.cs

.PHONY: proto clean
