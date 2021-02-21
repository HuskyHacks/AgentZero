import flask
from flask import *
from flask import current_app as app
from flask import render_template, request
from .models import db, Result
import base64
import json
import os


@app.route('/', methods=['GET'])
@app.route('/home', methods=['GET'])
@app.route('/index.html', methods=['GET'])
def index():
    results = Result.query.all()
    return render_template('index.html', title="AgentZero", results=results)


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


@app.route('/agents', methods=['GET', 'POST'])
def createAgent():
    return render_template('agents.html')


# TODO: Download function returns all agents in /agents dir. Need to figure out how to get an agent from the /agents
#  dir into dockerland because the app doesn't know about the /agents dir
@app.route('/return-files/', methods=['GET', 'POST'])
def return_files_tut():
    try:
        directory = "/home/app/web"
        for filename in os.listdir(directory):
            files = os.path.join(directory, filename)
            return files
    except Exception as e:
        return str(e), 500
