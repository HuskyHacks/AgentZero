from flask import current_app as app
from flask import render_template


@app.route('/', methods=['GET'])
@app.route('/index.html', methods=['GET'])
def index():
    return render_template('index.html', title="AgentZero")
