from flask.cli import FlaskGroup
from project import app
from project.app import db
from project.app.models import Result


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
    result = Result()
    result.host = "TEST"
    result.sourceIp = "10.0.0.5"
    result.sourcePort = "1738"
    result.user = "user"
    result.ntlmV2Hash = "Matt::COMMANDO:2BE5EFF73AA6ADF6:948D00731BF6491290587F754A63E607:01010000000000006ADBF27348FFD601EBBB59043A3C3562000000000200100043004F004D004D0041004E0044004F000100100043004F004D004D0041004E0044004F000400100063006F006D006D0061006E0064006F000300100063006F006D006D0061006E0064006F00070008006ADBF27348FFD601060004000200000008003000300000000000000001000000002000004B73219E92694599AA1CB49E7C7C222FC5E4C1B425F4412F5ACDA0C0610FF6730A001000000000000000000000000000000000000900160063006900660073002F0073006E0061007200650031000000000000000000"
    db.session.add(result)
    db.session.commit()


if __name__ == '__main__':
    cli()
