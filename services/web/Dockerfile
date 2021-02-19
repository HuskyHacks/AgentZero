FROM python:3.8.1-slim-buster as builder
WORKDIR /usr/src/app
ENV PYTHONDONTWRITEBYTECODE 1
ENV PYTHONBUFFERED 1
RUN apt-get update && apt-get install -y --no-install-recommends gcc

RUN pip install --upgrade pip
RUN pip install flake8
RUN apt install mono-complete
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
USER app
ENTRYPOINT ["/home/app/web/entrypoint.sh"]
