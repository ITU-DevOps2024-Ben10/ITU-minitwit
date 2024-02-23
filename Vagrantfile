# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config| # Note: Ensure you're using the latest configuration version

  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/id_rsa'
  config.vm.synced_folder ".", "/vagrant", type: "rsync"

  config.vm.define "webserver", primary: true do |server|
    # Correctly configure the DigitalOcean provider
    server.vm.provider :digital_ocean do |provider, override|
      provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
      provider.image = 'ubuntu-22-04-x64'
      provider.region = 'fra1'
      provider.size = 's-1vcpu-1gb'
      provider.privatenetworking = false
    end

    server.vm.hostname = "webserver"

    server.vm.provision "shell", inline: <<-SHELL

      echo "Installing .NET 8 and ASP.NET Core..."

      sudo apt-get install -y dotnet-sdk-8.0
      sudo apt-get install -y aspnetcore-runtime-8.0

      echo "Copying C# Project files..."

      cp -r /vagrant/src $HOME/ITU-minitwit # Make sure this path is correct

      echo "Running C# Project..."

      cd $HOME/ITU-minitwit/Minitwit.Web

      THIS_IP=`hostname -I | cut -d" " -f1`

      export ASPNETCORE_URLS=http://{$THIS_IP}:8080;https://{$THIS_IP}:8081;
      
      dotnet restore
      dotnet build
      #nohup dotnet run > dotnet_app.log &

      echo "================================================================="
      echo "=                            DONE                               ="
      echo "================================================================="
      echo "The webserver is exposing HTTPS on http://${THIS_IP}:8080"
      echo "The webserver is exposing HTTPS on http://${THIS_IP}:8080"
    SHELL
  end

  config.vm.provision "shell", privileged: false, inline: <<-SHELL
    sudo apt-get update
  SHELL
end
