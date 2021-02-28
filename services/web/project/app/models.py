from . import db


class User(db.Model):
    __tablename__ = "users"
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(128))
    password = db.Column(db.String(1024))


class Result(db.Model):
    __tablename__ = "results"
    id = db.Column(db.Integer, primary_key=True)
    sourceIP = db.Column(db.String(128))
    sourcePort = db.Column(db.String(128))
    user = db.Column(db.String(128))
    host = db.Column(db.String(128))
    ntlmV2Hash = db.Column(db.String(1024))
    ntlmV1Hash = db.Column(db.String(1024))
    clearText = db.Column(db.String(512))
    protocol = db.Column(db.String(128))
    protocolPort = db.Column(db.String(128))
    domain = db.Column(db.String(128))
    httpType = db.Column(db.String(128))
    httpPort = db.Column(db.String(128))
    httpSourceIP = db.Column(db.String(128))
    httpSourcePort = db.Column(db.String(128))
