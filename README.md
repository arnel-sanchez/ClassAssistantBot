# ClassAssistantBot
Bot de Telegram que permite ayudar en las clases a distancia

## [Intalar Docker](https://docs.docker.com/engine/install/)
## [Intalar Git](https://git-scm.com/book/es/v2/Inicio---Sobre-el-Control-de-Versiones-Instalaci%C3%B3n-de-Git)
## [Intalar Docker-Compose](https://github.com/docker/compose)
## [Intalar Postgresql](https://www.postgresql.org/download/linux/ubuntu/)
```bash
$ git clone https://github.com/arnel-sanchez/ClassAssistantBot
$ cd ClassAssistantBot/
$ docker-compose build && docker-compose up
$ docker-compose stop
$ docker-compose restart
$ docker-compose exec api dotnet ef database update --project ClassAssistantBot.csproj --no-build
$ docker-compose stop
$ docker-compose up
```
