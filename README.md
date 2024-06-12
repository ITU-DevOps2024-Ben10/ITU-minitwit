[![Codeac](https://static.codeac.io/badges/2-751821441.svg "Codeac")](https://app.codeac.io/github/ITU-DevOps2024-Ben10/ITU-minitwit)

# ITU-minitwit


## Estimation of Technical debt and maintainability
Code Complete:
https://codeclimate.com/github/ITU-DevOps2024-Ben10/ITU-minitwit

Sonarqube:
https://sonarcloud.io/organizations/itu-devops2024-ben10/projects?sort=analysis_date

# Feedback from Exam
All developers passed. :-) 
## 1 Truth

A requirement that the repository of an organization must fulfill is that it tells 1 truth, the "correct" truth. In our case we show multiple truths in the sense that the docker-compose/docker-stack script isn't corresponding to the one actually used in production. 

## Docker

We're not utilizing Docker enough. Docker supports health-checks, that like dependabot keeps dependencies updated and in that sense negates some of the risks that we're currently taken. As far as we know, the only dependencies updated are the ones in the dotnet environment due to the scope of dependabot. 

We're currently unnecessarily exposing the endpoint of the Prometheus server, it doesn't need a public port as it only needs to communicate with the Grafana Service, which can be done through the Docker Network that they both are a part of. 

The Dockerize command for the MiniTwit application, should definitely be reworked, as it works on imaginary grounds(GPT). 

The Dockerfile should be refactorized to abide på the Single Responisbilty Principle, splitting up the building part and the staging part. Also not a great idea to keep licensing credentials stored in it.

## Workflows
Choose a naming convention and stick to it...

In the workflows we also got critiqued for having multiple responsibilities collected in a single workflow.
Specifically build and test should be split into "build" and "test", and the order of the steps should be revisited, especially to ensure that testing happens AFTER any potential code is added to program. 

The testing workflow doesn't guarantee that it is in fact the newest version of minitwit that it test on due to the way the image being tested is defined. 

## Provisioning

When we found that updating the Vagrantfile was "too cumbersome" we should have instead opted for another approach than simply using the GUI provided by DigitalOcean. We could have done so through a script in the DigitalOcean CLI, or via their web-API through a DigitalOcean wrapper. 

## API

Way too large, should be refactored. 
Inconsistency between the docker-compose script 