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
    Name string
    Kind Kind
    Hash MD5
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

func ListenToEcho(conn *nats.Conn, echo Echo) {
    requestCodec, requestCodecError := TestRecord.Codec(TestRecord {})
    if requestCodecError != nil {
        panic(requestCodecError)
    }
    responseCodec, responseCodedError := TestRecord.Codec(TestRecord {})
    if responseCodedError != nil {
        panic(responseCodedError)
    }
    conn.Subscribe("", func(msg *nats.Msg) { 
        message, _, messageError := requestCodec.NativeFromBinary(msg.Data);
        if messageError != nil {
            msg.Term()
        } 
        if messageValue, ok := message.(TestRecord); ok {
            response := echo(messageValue);
            if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {
                msg.Respond(responseEncoded)
            } else {
                msg.Term()
            }
        } else {
            msg.Term()
        }
    })

}

type Echo2 func(testRecord TestRecord) (TestRecord, *TestError)

func ListenToEcho2(conn *nats.Conn, echo2 Echo2) {
    requestCodec, requestCodecError := TestRecord.Codec(TestRecord {})
    if requestCodecError != nil {
        panic(requestCodecError)
    }
    responseCodec, responseCodedError := TestRecord.Codec(TestRecord {})
    if responseCodedError != nil {
        panic(responseCodedError)
    }
    errorCodec, errorCodecError := TestError.Codec(TestError {})
    if errorCodecError != nil {
        panic(errorCodecError)
    }
    conn.Subscribe("", func(msg *nats.Msg) { 
        message, _, messageError := requestCodec.NativeFromBinary(msg.Data);
        if messageError != nil {
            msg.Term()
        } 
        if messageValue, ok := message.(TestRecord); ok {
            if response, responseError := echo2(messageValue); responseError == nil {
                if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {
                    msg.Respond(responseEncoded)
                } else {
                    msg.Term()
                }
            } else {
                if errorEncoded, err := errorCodec.BinaryFromNative(nil, responseError); err == nil {
                    msg.Respond(errorEncoded)
                } else {
                    msg.Term()
                }
            }
        } else {
            msg.Term()
        }
    })

}

type Echo3 func(testRecord TestRecord) *TestError

func ListenToEcho3(conn *nats.Conn, echo3 Echo3) {
    requestCodec, requestCodecError := TestRecord.Codec(TestRecord {})
    if requestCodecError != nil {
        panic(requestCodecError)
    }
    conn.Subscribe("", func(msg *nats.Msg) { 
        message, _, messageError := requestCodec.NativeFromBinary(msg.Data);
            if messageError != nil {
                msg.Term();
            } 
            if messageValue, ok := message.(TestRecord); ok {
                if error := echo3(messageValue); error == nil {
                msg.Ack()
            } else {
                msg.Term()
            }
            } else {
                msg.Term()
            }
    })

}

type Echo4 func() (TestRecord, *TestError)

func ListenToEcho4(conn *nats.Conn, echo4 Echo4) {
    responseCodec, responseCodedError := TestRecord.Codec(TestRecord {})
    if responseCodedError != nil {
        panic(responseCodedError)
    }
    errorCodec, errorCodecError := TestError.Codec(TestError {})
    if errorCodecError != nil {
        panic(errorCodecError)
    }
    conn.Subscribe("", func(msg *nats.Msg) { 
        if response, responseError := echo4(); responseError == nil {
            if responseEncoded, err := responseCodec.BinaryFromNative(nil, response); err == nil {
                msg.Respond(responseEncoded)
            } else {
                msg.Term()
            }
        } else {
            if errorEncoded, err := errorCodec.BinaryFromNative(nil, responseError); err == nil {
                msg.Respond(errorEncoded)
            } else {
                msg.Term()
            }
        }
    })

}

type Echo5 func()

func ListenToEcho5(conn *nats.Conn, echo5 Echo5) {
    conn.Subscribe("", func(msg *nats.Msg) { 
        echo5()
            msg.Ack()
    })

}

type Echo6 func() *TestError

func ListenToEcho6(conn *nats.Conn, echo6 Echo6) {
    conn.Subscribe("", func(msg *nats.Msg) { 
        if error := echo6(); error == nil {
            msg.Ack()
        } else {
            msg.Term()
        }
    })

}

type Error func() *TestError

func ListenToError(conn *nats.Conn, error Error) {
    conn.Subscribe("", func(msg *nats.Msg) { 
        if error := error(); error == nil {
            msg.Ack()
        } else {
            msg.Term()
        }
    })

}

type Ping func()

func ListenToPing(conn *nats.Conn, ping Ping) {
    conn.Subscribe("", func(msg *nats.Msg) { 
        ping()
            msg.Ack()
    })

}
