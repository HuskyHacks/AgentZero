from . import db


class Host(db.Model):
    __tablename__ = "host"
    id = db.Column(db.Integer, primary_key=True)
    address = db.Column(db.String(128))
    ntlmv2 = db.relationship('Ntlmv2', backref='host')
    ntlmv1 = db.relationship('Ntlmv1', backref='host')
    cleartext = db.relationship('Cleartext', backref='host')


class Ntlmv2(db.Model):
    __tablename__ = "ntlmv2"
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(256))
    hash = db.Column(db.String(256))
    result_id = db.Column(db.Integer, db.ForeignKey('host.id'))


class Ntlmv1(db.Model):
    __tablename__ = "ntlmv1"
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(256))
    hash = db.Column(db.String(256))
    result_id = db.Column(db.Integer, db.ForeignKey('host.id'))


class Cleartext(db.Model):
    __tablename__ = "cleartext"
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(256))
    password = db.Column(db.String(256))
    result_id = db.Column(db.Integer, db.ForeignKey('host.id'))
