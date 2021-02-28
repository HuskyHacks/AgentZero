import flask
from flask import *
from flask import current_app as app
from flask import render_template, request
from werkzeug.security import check_password_hash
from .models import db, Result, User
from flask import send_from_directory
import base64
import json
import os
from .GenerateAgent import *
from flask_nav import Nav
from flask_nav.elements import Navbar, View
from passlib.hash import sha256_crypt

DOWNLOAD_DIRECTORY = "/home/app/web/project/app/agents/"

nav = Nav()


@nav.navigation()
def mynavbar():
    return Navbar(
        'AgentZero',
        View('Home', 'index'),
        View('Listener', 'listener'),
        View('Agents', 'createAgent'),
        View('Accounts', 'accounts')
    )


@app.route('/', methods=['GET', 'POST'])
def login():
    if flask.request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')
        user = User.query.filter_by(username=username).first()

        if not user or not check_password_hash(user.password, password):
            error = "Please check your login details and try again."
            return render_template('login.html', error=error)
        return redirect(url_for('index'))
    else:
        return render_template('login.html')






@app.route('/home', methods=['GET'])
@app.route('/index.html', methods=['GET'])
def index():
    results = Result.query.all()
    return render_template('index.html', title="AgentZero", results=results)


@app.route('/accounts.html', methods=['GET'])
def accounts():
    users = User.query.all()
    return render_template('accounts.html', title="AgentZero", users=users)


# TODO: Listener name generation matches Agent name generation

@app.route('/listener', methods=['GET', 'POST'])
def listener():
    if flask.request.method == 'POST':
        try:
            b64content = request.get_data()
            content = base64.b64decode(b64content).decode("UTF-8")
            content = json.loads(content)
        except Exception as e:
            return str(e), 500
        if content:
            result = Result()
            if 'host' in content:
                result.host = content['host']
            if 'domain' in content:
                result.domain = content['domain']
            if 'sourcePort' in content:
                result.sourcePort = content['sourcePort']
            if 'sourceIP' in content:
                result.sourceIP = content['sourceIP']
            if 'user' in content:
                result.user = content['user']
            if 'protocol' in content:
                result.protocol = content['protocol']
            if 'protocolPort' in content:
                result.protocolPort = content['protocolPort']
            if 'ntlmV2Hash' in content:
                result.ntlmV2Hash = content['ntlmV2Hash']
            if 'ntlmV1Hash' in content:
                result.ntlmV1Hash = content['ntlmV1Hash']
            if 'httpType' in content:
                result.httpType = content['httpType']
            if 'httpPort' in content:
                result.httpPort = content['httpPort']
            if 'httpSourceIP' in content:
                result.httpSourceIP = content['httpSourceIP']
            if 'httpSourcePort' in content:
                result.httpSourcePort = content['httpSourcePort']
            if 'clearText' in content:
                result.clearText = content['clearText']
            try:
                # TODO: if the hash already exists, don't write to DB
                # repeat = db.session.query(result).first()
                # if not repeat:
                db.session.add(result)
                db.session.commit()
            except Exception as e:
                return str(e), 500
            return "OK", 200
        return "MESSAGE: {0}".format(request.is_json)
    else:
        return render_template('listener.html')


# TODO: Download function returns all agents in /agents dir.
@app.route('/return-files/', methods=['GET', 'POST'])
def list_files():
    """Endpoint to list files on the server."""
    files = []
    for filename in os.listdir(DOWNLOAD_DIRECTORY):
        path = os.path.join(DOWNLOAD_DIRECTORY, filename)
        if os.path.isfile(path):
            files.append(filename)
    return jsonify(files)


@app.route("/files/<path:path>")
def get_file(path):
    return send_from_directory(DOWNLOAD_DIRECTORY, path, as_attachment=True)


# TODO: agent generation
@app.route('/agents', methods=['GET', 'POST'])
def createAgent():
    #    if flask.request.method == 'POST':
    #        create_agent("10.10.1.149", "1776", "/home/app/web/project/app/static/")
    #        moveAndRename(agentNameGenerator(8))
    #    else:
    return render_template('agents.html'), 200


@app.route('/favicon.ico')
def favicon():
    return send_from_directory(os.path.join(app.root_path, 'static'),
                               'favicon.ico', mimetype='image/vnd.microsoft.icon')


nav.init_app(app)
