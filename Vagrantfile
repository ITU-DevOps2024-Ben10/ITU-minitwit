# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config| # Note: Ensure you're using the latest configuration version

  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/do_ssh_key'
  config.vm.synced_folder ".", "/vagrant", type: "rsync"
  config.vm.provision "shell", inline: <<-SHELL
      systemctl disable apt-daily.service
      systemctl disable apt-daily.timer
    SHELL

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

      echo "Updating host machine and adding Microsoft packages to Aptitude"

      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb

      # Ensure APT lock files are not held
      while sudo fuser /var/lib/dpkg/lock >/dev/null 2>&1 || sudo fuser /var/lib/apt/lists/lock >/dev/null 2>&1 || sudo fuser /var/cache/apt/archives/lock >/dev/null 2>&1; do
        echo "Waiting for other APT processes to finish..."
        sleep 1
      done

      sudo apt-get update
      
      # Ensure APT lock files are not held
            while sudo fuser /var/lib/dpkg/lock >/dev/null 2>&1 || sudo fuser /var/lib/apt/lists/lock >/dev/null 2>&1 || sudo fuser /var/cache/apt/archives/lock >/dev/null 2>&1; do
              echo "Waiting for other APT processes to finish..."
              sleep 1
            done

      echo "Installing .NET 8 and ASP.NET Core..."

      sudo apt-get install -y dotnet-sdk-8.0
      sudo apt-get install -y aspnetcore-runtime-8.0

      echo "Copying C# Project files..."

      cp -r /vagrant/src $HOME/ITU-minitwit # Make sure this path is correct

      echo "Running C# Project..."

      THIS_IP=`hostname -I | cut -d" " -f1`
      
      dotnet restore $HOME/ITU-minitwit/Minitwit.Web
      dotnet publish $HOME/ITU-minitwit/Minitwit.Web -o webserver
      
      cd $HOME/webserver

      nohup ./Minitwit.Web --urls http://${THIS_IP}:8080;https://${THIS_IP}:8081 > dotnet_app.log &

      echo "================================================================="
      echo "=                            DONE                               ="
      echo "================================================================="
      echo "The webserver is exposing HTTPS on http://${THIS_IP}:8080"
      echo "The webserver is exposing HTTPS on http://${THIS_IP}:8081"
    SHELL
  end
end
