package Simple

import (
	"encoding/base64"

	"github.com/linkedin/goavro"
	"github.com/nats-io/nats.go"
)

func (foo Foo) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoicmVjb3JkIiwibmFtZSI6IkZvbyIsIm5hbWVzcGFjZSI6bnVsbCwic3ltYm9scyI6bnVsbCwib3JkZXIiOm51bGwsImFsaWFzZXMiOm51bGwsInNpemUiOm51bGwsImZpZWxkcyI6W3sibmFtZSI6ImxhYmVsIiwidHlwZSI6InN0cmluZyIsImFsaWFzZXMiOm51bGx9XX0="
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type Foo struct {
	Label string `avro:"label"`
}

type Kind int

const (
	A Kind = 0
	B Kind = 1
	C Kind = 2
)

type MD5 string

func (node Node) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoicmVjb3JkIiwibmFtZSI6Ik5vZGUiLCJuYW1lc3BhY2UiOm51bGwsInN5bWJvbHMiOm51bGwsIm9yZGVyIjpudWxsLCJhbGlhc2VzIjpudWxsLCJzaXplIjpudWxsLCJmaWVsZHMiOlt7Im5hbWUiOiJsYWJlbCIsInR5cGUiOiJzdHJpbmciLCJhbGlhc2VzIjpudWxsfSx7Im5hbWUiOiJjaGlsZHJlbiIsInR5cGUiOnsidHlwZSI6ImFycmF5IiwiaXRlbXMiOiJOb2RlIn0sImFsaWFzZXMiOm51bGx9XX0="
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type Node struct {
	Label    string `avro:"label"`
	Children []Node `avro:"children"`
}

func (interop Interop) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoicmVjb3JkIiwibmFtZSI6IkludGVyb3AiLCJuYW1lc3BhY2UiOm51bGwsInN5bWJvbHMiOm51bGwsIm9yZGVyIjpudWxsLCJhbGlhc2VzIjpudWxsLCJzaXplIjpudWxsLCJmaWVsZHMiOlt7Im5hbWUiOiJpbnRGaWVsZCIsInR5cGUiOiJpbnQiLCJhbGlhc2VzIjpudWxsfSx7Im5hbWUiOiJsb25nRmllbGQiLCJ0eXBlIjoibG9uZyIsImFsaWFzZXMiOm51bGx9LHsibmFtZSI6InN0cmluZ0ZpZWxkIiwidHlwZSI6InN0cmluZyIsImFsaWFzZXMiOm51bGx9LHsibmFtZSI6ImJvb2xGaWVsZCIsInR5cGUiOiJib29sZWFuIiwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoiZmxvYXRGaWVsZCIsInR5cGUiOiJmbG9hdCIsImFsaWFzZXMiOm51bGx9LHsibmFtZSI6ImRvdWJsZUZpZWxkIiwidHlwZSI6ImRvdWJsZSIsImFsaWFzZXMiOm51bGx9LHsibmFtZSI6Im51bGxGaWVsZCIsInR5cGUiOiJudWxsIiwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoiYXJyYXlGaWVsZCIsInR5cGUiOnsidHlwZSI6ImFycmF5IiwiaXRlbXMiOiJkb3VibGUifSwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoibWFwRmllbGQiLCJ0eXBlIjp7InR5cGUiOiJtYXAiLCJ2YWx1ZXMiOiJGb28ifSwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoidW5pb25GSWVsZCIsInR5cGUiOlsiYm9vbGVhbiIsImRvdWJsZSIseyJ0eXBlIjoiYXJyYXkiLCJpdGVtcyI6ImJ5dGVzIn1dLCJhbGlhc2VzIjpudWxsfSx7Im5hbWUiOiJlbnVtRmllbGQiLCJ0eXBlIjp7InR5cGUiOiJlbnVtIiwibmFtZSI6IktpbmQiLCJuYW1lc3BhY2UiOiJvcmcuYXBhY2hlLmF2cm8iLCJzeW1ib2xzIjpbIkEiLCJCIiwiQyJdfSwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoiZml4ZWRGaWVsZCIsInR5cGUiOnsidHlwZSI6ImZpeGVkIiwibmFtZSI6Ik1ENSIsIm5hbWVzcGFjZSI6Im9yZy5hcGFjaGUuYXZybyIsInNpemUiOjE2fSwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoicmVjb3JkRmllbGQiLCJ0eXBlIjp7InR5cGUiOiJyZWNvcmQiLCJuYW1lIjoiTm9kZSIsIm5hbWVzcGFjZSI6bnVsbCwic3ltYm9scyI6bnVsbCwib3JkZXIiOm51bGwsImFsaWFzZXMiOm51bGwsInNpemUiOm51bGwsImZpZWxkcyI6W3sibmFtZSI6ImxhYmVsIiwidHlwZSI6InN0cmluZyIsImFsaWFzZXMiOm51bGx9LHsibmFtZSI6ImNoaWxkcmVuIiwidHlwZSI6eyJ0eXBlIjoiYXJyYXkiLCJpdGVtcyI6Ik5vZGUifSwiYWxpYXNlcyI6bnVsbH1dfSwiYWxpYXNlcyI6bnVsbH1dfQ=="
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type Interop struct {
	IntField    int         `avro:"intField"`
	LongField   int64       `avro:"longField"`
	StringField string      `avro:"stringField"`
	BoolField   bool        `avro:"boolField"`
	FloatField  float32     `avro:"floatField"`
	DoubleField float64     `avro:"doubleField"`
	NullField   any         `avro:"nullField"`
	ArrayField  []float64   `avro:"arrayField"`
	MapField    map[Foo]any `avro:"mapField"`
	UnionFIeld  any         `avro:"unionFIeld"`
	EnumField   Kind        `avro:"enumField"`
	FixedField  MD5         `avro:"fixedField"`
	RecordField Node        `avro:"recordField"`
}

func (testRecord TestRecord) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoicmVjb3JkIiwibmFtZSI6IlRlc3RSZWNvcmQiLCJuYW1lc3BhY2UiOm51bGwsInN5bWJvbHMiOm51bGwsIm9yZGVyIjpudWxsLCJhbGlhc2VzIjpudWxsLCJzaXplIjpudWxsLCJmaWVsZHMiOlt7Im5hbWUiOiJuYW1lIiwidHlwZSI6eyJ0eXBlIjoic3RyaW5nIiwib3JkZXIiOiJpZ25vcmUifSwiYWxpYXNlcyI6bnVsbH0seyJuYW1lIjoia2luZCIsInR5cGUiOnsidHlwZSI6ImVudW0iLCJuYW1lIjoiS2luZCIsIm5hbWVzcGFjZSI6Im9yZy5hcGFjaGUuYXZybyIsInN5bWJvbHMiOlsiQSIsIkIiLCJDIl19LCJhbGlhc2VzIjpudWxsfSx7Im5hbWUiOiJoYXNoIiwidHlwZSI6eyJ0eXBlIjoiZml4ZWQiLCJuYW1lIjoiTUQ1IiwibmFtZXNwYWNlIjoib3JnLmFwYWNoZS5hdnJvIiwic2l6ZSI6MTZ9LCJhbGlhc2VzIjpudWxsfSx7Im5hbWUiOiJudWxsYWJsZUhhc2giLCJ0eXBlIjpbIk1ENSIsIm51bGwiXSwiYWxpYXNlcyI6WyJoYXNoIl19LHsibmFtZSI6ImFycmF5T2ZMb25ncyIsInR5cGUiOnsidHlwZSI6ImFycmF5IiwiaXRlbXMiOiJsb25nIn0sImFsaWFzZXMiOm51bGx9XX0="
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type TestRecord struct {
	Name         string  `avro:"name"`
	Kind         Kind    `avro:"kind"`
	Hash         MD5     `avro:"hash"`
	NullableHash *MD5    `avro:"nullableHash"`
	ArrayOfLongs []int64 `avro:"arrayOfLongs"`
}

func (testError TestError) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "eyJ0eXBlIjoiZXJyb3IiLCJuYW1lIjoiVGVzdEVycm9yIiwibmFtZXNwYWNlIjpudWxsLCJzeW1ib2xzIjpudWxsLCJvcmRlciI6bnVsbCwiYWxpYXNlcyI6bnVsbCwic2l6ZSI6bnVsbCwiZmllbGRzIjpbeyJuYW1lIjoibWVzc2FnZSIsInR5cGUiOiJzdHJpbmciLCJhbGlhc2VzIjpudWxsfV19"
	if value, err := base64.StdEncoding.DecodeString(avroSchemaInBase64); err == nil {
		if codec, err := goavro.NewCodec(string(value)); err == nil {
			return codec, nil
		} else {
			return nil, err
		}
	} else {
		return nil, err
	}
}

type TestError struct {
	Message string `avro:"message"`
}

type Echo func(testRecord TestRecord) (*TestRecord, error)

func EchoClient(conn *nats.Conn) Echo {

	requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
	if requestCodecError != nil {
		panic(requestCodecError)
	}

	responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
	if responseCodecError != nil {
		panic(responseCodecError)
	}

	return func(testRecord TestRecord) (*TestRecord, error) {
		if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, testRecord); requestEncodedError == nil {
			if response, error := conn.Request("$namespace", requestEncoded, time.Second*10); error == nil {
				if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
					if response, ok := responseDecoded.(TestRecord); ok {
						return &response, nil
					} else {
						return nil, errors.New("Invalid message type")
					}
				} else {
					return nil, responseDecodedError
				}
			} else {
				return nil, error
			}
		} else {
			return nil, requestEncodedError
		}
	}

}

type Echo2 func(testRecord TestRecord) (*TestRecord, *TestError, error)

func Echo2Client(conn *nats.Conn) Echo2 {

	requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
	if requestCodecError != nil {
		panic(requestCodecError)
	}

	responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
	if responseCodecError != nil {
		panic(responseCodecError)
	}

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func(testRecord TestRecord) (*TestRecord, *TestError, error) {
		if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, testRecord); requestEncodedError == nil {
			if response, error := conn.Request("$namespace", requestEncoded, time.Second*10); error == nil {
				if response.Header.Get("Status") == "200" {
					if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
						if response, ok := responseDecoded.(TestRecord); ok {
							return &response, nil, nil
						} else {
							return nil, nil, errors.New("Invalid message type")
						}
					} else {
						return nil, nil, responseDecodedError
					}
				} else {
					if errorDecoded, _, errorDecodedError := errorCodec.NativeFromBinary(response.Data); errorDecodedError == nil {
						if error, ok := errorDecoded.(TestError); ok {
							return nil, &error, nil
						} else {
							return nil, nil, errors.New("Invalid error type")
						}
					} else {
						return nil, nil, errorDecodedError
					}
				}

			} else {
				return nil, nil, error
			}
		} else {
			return nil, nil, requestEncodedError
		}
	}

}

type Echo3 func(testRecord TestRecord) error

func Echo3Client(conn *nats.Conn) Echo3 {

	requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
	if requestCodecError != nil {
		panic(requestCodecError)
	}

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func(testRecord TestRecord) error {
		if requestEncoded, requestEncodedError := requestCodec.BinaryFromNative(nil, testRecord); requestEncodedError == nil {
			return conn.Publish("$namespace", requestEncoded)
		} else {
			return requestEncodedError
		}
	}

}

type Echo4 func() (*TestRecord, *TestError, error)

func Echo4Client(conn *nats.Conn) Echo4 {

	responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
	if responseCodecError != nil {
		panic(responseCodecError)
	}

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func() (*TestRecord, *TestError, error) {
		if response, error := conn.Request("$namespace", []byte{}, time.Second*10); error == nil {
			if response.Header.Get("Status") == "200" {
				if responseDecoded, _, responseDecodedError := responseCodec.NativeFromBinary(response.Data); responseDecodedError == nil {
					if response, ok := responseDecoded.(TestRecord); ok {
						return &response, nil, nil
					} else {
						return nil, nil, errors.New("Invalid message type")
					}
				} else {
					return nil, nil, responseDecodedError
				}
			} else {
				if errorDecoded, _, errorDecodedError := errorCodec.NativeFromBinary(response.Data); errorDecodedError == nil {
					if error, ok := errorDecoded.(TestError); ok {
						return nil, &error, nil
					} else {
						return nil, nil, errors.New("Invalid error type")
					}
				} else {
					return nil, nil, errorDecodedError
				}
			}

		} else {
			return nil, nil, error
		}
	}

}

type Echo5 func() error

func Echo5Client(conn *nats.Conn) Echo5 {

	return func() error {
		return conn.Publish("$namespace", []byte{})
	}

}

type Echo6 func() error

func Echo6Client(conn *nats.Conn) Echo6 {

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func() error {
		return conn.Publish("$namespace", []byte{})
	}

}

type Error func() error

func ErrorClient(conn *nats.Conn) Error {

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func() error {
		return conn.Publish("$namespace", []byte{})
	}

}

type Ping func() error

func PingClient(conn *nats.Conn) Ping {

	return func() error {
		return conn.Publish("$namespace", []byte{})
	}

}
