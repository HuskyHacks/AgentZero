FROM python:3.8.1-slim-buster as builder
WORKDIR /usr/src/app
ENV PYTHONDONTWRITEBYTECODE 1
ENV PYTHONBUFFERED 1
RUN apt-get update && apt-get install -y --no-install-recommends gcc
RUN apt-get update && apt-get install -y gnupg2

# Installing Mono-Complete
RUN apt-get update -q
RUN apt-get -y -q install wget
RUN wget http://download.mono-project.com/repo/xamarin.gpg
RUN apt-key add xamarin.gpg
RUN rm xamarin.gpg
RUN echo "deb http://download.mono-project.com/repo/debian wheezy main" > /etc/apt/sources.list.d/mono-xamarin.list
RUN apt-get update -q


RUN pip install --upgrade pip
RUN pip install flake8

COPY . /usr/src/app/

COPY ./requirements.txt .
RUN pip wheel --no-cache-dir --no-deps --wheel-dir /usr/src/app/wheels -r requirements.txt

FROM python:3.8.1-slim-buster

RUN mkdir -p /home/app
RUN addgroup app
RUN useradd -s /bin/bash -g app app

ENV HOME=/home/app
ENV APP_HOME=/home/app/web
RUN mkdir $APP_HOME
WORKDIR $APP_HOME
RUN apt-get update && apt-get install -y --no-install-recommends netcat
COPY --from=builder /usr/src/app/wheels /wheels
COPY --from=builder /usr/src/app/requirements.txt .
RUN pip install --upgrade pip
RUN pip install --no-cache /wheels/*
COPY ./entrypoint.sh $APP_HOME
COPY . $APP_HOME
RUN chown -R app:app $APP_HOME
RUN apt-get install -y mono-xbuild
USER app
ENTRYPOINT ["/home/app/web/entrypoint.sh"]
