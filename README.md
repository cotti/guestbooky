<p align="center">
  <a href="" rel="noopener">
 <img width=200px height=200px src="docs/guestbooky.png" alt="Guestbooky Project logo"></a>
</p>

<h3 align="center">Guestbooky</h3>

<div align="center">

[![Status](https://img.shields.io/badge/status-active-success.svg)]()
[![GitHub Issues](https://img.shields.io/github/issues/cotti/guestbooky.svg)](https://github.com/cotti/guestbooky/issues)
[![GitHub Pull Requests](https://img.shields.io/github/issues-pr/cotti/guestbooky.svg)](https://github.com/cotti/guestbooky/pulls)
[![License](https://img.shields.io/badge/license-AGPLv3-003300.svg)](/LICENSE)

</div>

---


<p align="center">A simple yet somehow overdesigned guestbook system featuring a simple control panel <small>(which is a WIP so you'll have to make do with a db manager)</small></p>

<p align="center"> This is phase I of the personal backscratchers project.</p>

## 📝 Table of Contents


- [📝 Table of Contents](#-table-of-contents)
- [🧐 About ](#-about-)
- [📑 Documentation ](#-documentation-)
- [🏁 Getting Started ](#-getting-started-)
- [🕸️ Prerequisites](#️-prerequisites)
- [🚀 Deployment ](#-deployment-)
- [⛏️ Built Using ](#️-built-using-)
- [✍️ Authors ](#️-authors-)

## 🧐 About <a name = "about"></a>

I really need to get my hands dirty from time to time, so I figured I'd make a guestbook for my marriage hotsite. And make everyone else see this code.

It includes many concepts that are very reasonable to tinker with as learning material, in a bite-sized project complexity that allows me to talk about it without losing the breadcrumb trail.

## 📑 Documentation <a name = "documentation"></a>

[Comments and general documentation/musings on the project](docs/comments.md)

## 🏁 Getting Started <a name = "getting_started"></a>

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See [deployment](#deployment) for notes on how to deploy the project on a live system.

## 🕸️ Prerequisites

For running it locally:
- .NET 8.0
- A running instance of MongoDB
- A Cloudflare turnstile secret key for the captcha
- Not forgetting to set up environment variables

You will be able to see in `build/docker-compose.public.yml` that the application makes heavy usage of them.
```
      - ASPNETCORE_ENVIRONMENT=Production
      - CORS_ORIGINS=https://guestbook.example.com,http://localhost:5008,http://localhost:8080
      - ACCESS_USERNAME=user
      - ACCESS_PASSWORD=pass
      - ACCESS_TOKENKEY=pleaseinsertafairlylargetokenkeyherewillyou
      - ACCESS_ISSUER=https://guestbook.example.com/api
      - ACCESS_AUDIENCE=https://guestbook.example.com
      - CLOUDFLARE_SECRET=0x000000000000000000000000000000000
      - MONGODB_CONNECTIONSTRING=mongodb://mongouser:mongopass@mongo:27017/Guestbooky
      - MONGODB_DATABASENAME=Guestbooky
      - LOG_LEVEL=Debug
```

> [!IMPORTANT]
 You will need to set them up either by hand or by using your IDE's capabilities. On Visual Studio, that can be done via the Debug Properties of Guestbooky.API.

|Env Variable Keys|Usage|
|----|----|
|**CORS_ORIGINS**, **ACCESS_\***|Variables related to JWT issuing and checking. In order to use the GET and DELETE endpoints for the messages, you need to use a bearer token.|
|**CLOUDFLARE_SECRET**|The turnstile secret, used in the server portion of the captcha check.|
|**MONGODB_\***|Related to the connection to MongoDB. Yeah.|
|**LOG_\***|Logging.|


> [!TIP]
> For local usage of the backend, you can use `docker-compose.local.yml` and edit the fields you need.

## 🚀 Deployment <a name = "deployment"></a>

Use `docker-compose.public.yml` as a basis. it should create the image for you and start running.

## ⛏️ Built Using <a name = "built_using"></a>

- [MongoDB](https://www.mongodb.com/) - Database
- [.NET](https://dot.net/) - Backend
- [Cloudflare Turnstile](https://www.cloudflare.com/pt-br/products/turnstile/) - Captcha

## ✍️ Authors <a name = "authors"></a>

- [@cotti](https://github.com/cotti) | [cotti.com.br](https://cotti.com.br)