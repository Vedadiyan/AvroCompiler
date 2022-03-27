package Simple

import (
	"encoding/base64"

	"github.com/linkedin/goavro"
	"github.com/nats-io/nats.go"
)

type Kind int

const (
	FOO Kind = 0
	BAR Kind = 1
	BAZ Kind = 2
)

type MD5 string

func (testRecord TestRecord) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "ew0KICAgICJ0eXBlIiA6ICJyZWNvcmQiLA0KICAgICJuYW1lIiA6ICJUZXN0UmVjb3JkIiwNCiAgICAiZmllbGRzIiA6IFsgew0KICAgICAgIm5hbWUiIDogIm5hbWUiLA0KICAgICAgInR5cGUiIDogew0KICAgICAgICAidHlwZSIgOiAic3RyaW5nIiwNCiAgICAgICAgIm9yZGVyIiA6ICJpZ25vcmUiDQogICAgICB9DQogICAgfSwgew0KICAgICAgIm5hbWUiIDogImtpbmQiLA0KICAgICAgInR5cGUiIDogIktpbmQiDQogICAgfSwgew0KICAgICAgIm5hbWUiIDogImhhc2giLA0KICAgICAgInR5cGUiIDogIk1ENSINCiAgICB9LCB7DQogICAgICAibmFtZSIgOiAibnVsbGFibGVIYXNoIiwNCiAgICAgICJ0eXBlIiA6IFsgIk1ENSIsICJudWxsIiBdLA0KICAgICAgImFsaWFzZXMiIDogWyAiaGFzaCIgXQ0KICAgIH0sIHsNCiAgICAgICJuYW1lIiA6ICJhcnJheU9mTG9uZ3MiLA0KICAgICAgInR5cGUiIDogew0KICAgICAgICAidHlwZSIgOiAiYXJyYXkiLA0KICAgICAgICAiaXRlbXMiIDogImxvbmciDQogICAgICB9DQogICAgfSBdDQogIH0="
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
	Name         string
	Kind         Kind
	Hash         MD5
	NullableHash *MD5
	ArrayOfLongs []int64
}

func (testError TestError) Codec() (*goavro.Codec, error) {
	avroSchemaInBase64 := "ew0KICAgICJ0eXBlIiA6ICJlcnJvciIsDQogICAgIm5hbWUiIDogIlRlc3RFcnJvciIsDQogICAgImZpZWxkcyIgOiBbIHsNCiAgICAgICJuYW1lIiA6ICJtZXNzYWdlIiwNCiAgICAgICJ0eXBlIiA6ICJzdHJpbmciDQogICAgfSBdDQogIH0="
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
	Message string
}

type Echo func(testRecord TestRecord) TestRecord

func EchoListener(conn *nats.Conn) func(echo Echo) (*nats.Subscription, error) {

	requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
	if requestCodecError != nil {
		panic(requestCodecError)
	}

	responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
	if responseCodecError != nil {
		panic(responseCodecError)
	}

	return func(echo Echo) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			if requestDecoded, _, requestDecodingError := requestCodec.NativeFromBinary(msg.Data); requestDecodingError == nil {
				if request, ok := requestDecoded.(TestRecord); ok {

					response := echo(request)

					if responseEncoded, responseEncodingError := responseCodec.BinaryFromNative(nil, response); responseEncodingError == nil {
						msg.Respond(responseEncoded)
					} else {
						msg.Term()
					}

				} else {
					msg.Term()
				}
			} else {
				msg.Term()
			}
		})
	}
}

type Echo2 func(testRecord TestRecord) (TestRecord, *TestError)

func Echo2Listener(conn *nats.Conn) func(echo2 Echo2) (*nats.Subscription, error) {

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

	return func(echo2 Echo2) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			if requestDecoded, _, requestDecodingError := requestCodec.NativeFromBinary(msg.Data); requestDecodingError == nil {
				if request, ok := requestDecoded.(TestRecord); ok {

					if response, err := echo2(request); err == nil {

						if responseEncoded, responseEncodingError := responseCodec.BinaryFromNative(nil, response); responseEncodingError == nil {
							msg.Respond(responseEncoded)
						} else {
							msg.Term()
						}

					} else {

						if errorEncoded, errorEncodingError := errorCodec.BinaryFromNative(nil, err); errorEncodingError == nil {
							msg.Respond(errorEncoded)
						} else {
							msg.Term()
						}

					}

				} else {
					msg.Term()
				}
			} else {
				msg.Term()
			}
		})
	}
}

type Echo3 func(testRecord TestRecord) *TestError

func Echo3Listener(conn *nats.Conn) func(echo3 Echo3) (*nats.Subscription, error) {

	requestCodec, requestCodecError := TestRecord.Codec(TestRecord{})
	if requestCodecError != nil {
		panic(requestCodecError)
	}

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func(echo3 Echo3) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			if requestDecoded, _, requestDecodingError := requestCodec.NativeFromBinary(msg.Data); requestDecodingError == nil {
				if request, ok := requestDecoded.(TestRecord); ok {

					if err := echo3(request); err != nil {

						if errorEncoded, errorEncodingError := errorCodec.BinaryFromNative(nil, err); errorEncodingError == nil {
							msg.Respond(errorEncoded)
						} else {
							msg.Term()
						}

					}

				} else {
					msg.Term()
				}
			} else {
				msg.Term()
			}
		})
	}
}

type Echo4 func() (TestRecord, *TestError)

func Echo4Listener(conn *nats.Conn) func(echo4 Echo4) (*nats.Subscription, error) {

	responseCodec, responseCodecError := TestRecord.Codec(TestRecord{})
	if responseCodecError != nil {
		panic(responseCodecError)
	}

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func(echo4 Echo4) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			if response, err := echo4(); err == nil {

				if responseEncoded, responseEncodingError := responseCodec.BinaryFromNative(nil, response); responseEncodingError == nil {
					msg.Respond(responseEncoded)
				} else {
					msg.Term()
				}

			} else {

				if errorEncoded, errorEncodingError := errorCodec.BinaryFromNative(nil, err); errorEncodingError == nil {
					msg.Respond(errorEncoded)
				} else {
					msg.Term()
				}

			}

		})
	}
}

type Echo5 func()

func Echo5Listener(conn *nats.Conn) func(echo5 Echo5) (*nats.Subscription, error) {

	return func(echo5 Echo5) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			echo5()

		})
	}
}

type Echo6 func() *TestError

func Echo6Listener(conn *nats.Conn) func(echo6 Echo6) (*nats.Subscription, error) {

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func(echo6 Echo6) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			if err := echo6(); err != nil {

				if errorEncoded, errorEncodingError := errorCodec.BinaryFromNative(nil, err); errorEncodingError == nil {
					msg.Respond(errorEncoded)
				} else {
					msg.Term()
				}

			}

		})
	}
}

type Error func() *TestError

func ErrorListener(conn *nats.Conn) func(error Error) (*nats.Subscription, error) {

	errorCodec, errorCodecError := TestError.Codec(TestError{})
	if errorCodecError != nil {
		panic(errorCodec)
	}

	return func(error Error) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			if err := error(); err != nil {

				if errorEncoded, errorEncodingError := errorCodec.BinaryFromNative(nil, err); errorEncodingError == nil {
					msg.Respond(errorEncoded)
				} else {
					msg.Term()
				}

			}

		})
	}
}

type Ping func()

func PingListener(conn *nats.Conn) func(ping Ping) (*nats.Subscription, error) {

	return func(ping Ping) (*nats.Subscription, error) {
		return conn.Subscribe("$namespace", func(msg *nats.Msg) {

			ping()

		})
	}
}
