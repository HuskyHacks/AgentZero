from flask.cli import FlaskGroup
from project import app
from project.app import db
from project.app.models import Host, Ntlmv1, Ntlmv2, Cleartext


cli = FlaskGroup(app)


@cli.command("create_db")
def create_db():
    try:
        db.drop_all()
        db.create_all()
        db.session.commit()
    except Exception as e:
        print(str(e))


@cli.command("seed_db")
def seed_db():
    host = Host(address="192.168.73.123", 
                ntlmv2="admin::N46iSNekpT:08ca45b7d7ea58ee:88dcbe4446168966a153a0064958dac6:5c7830315c7830310000000000000b45c67103d07d7b95acd12ffa11230e0000000052920b85f78d013c31cdb3b92f5d765c783030")
    db.session.add(host)
    db.session.commit()


if __name__ == '__main__':
    cli()
