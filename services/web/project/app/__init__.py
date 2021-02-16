from flask import Flask
from flask_sqlalchemy import SQLAlchemy

db = SQLAlchemy()


def create_app():
    app = Flask(__name__, instance_relative_config=False)
    app.config.from_object('project.config.Config')
    app.secret_key = ""

    db.init_app(app)
    with app.app_context():
        from . import views

    return app
