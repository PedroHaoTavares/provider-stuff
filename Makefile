VERSION ?= 1.3.1.0
TARGET_ABI ?= 10.11.0.0
GITHUB_REPO ?= PedroHaoTavares/provider-stuff
ASSET := providerstuff-$(VERSION).zip
PUBLISH_DIR := build/publish
RELEASE_DIR := release

.PHONY: restore test publish package checksum manifest clean

restore:
	dotnet restore Jellyfin.Plugin.ProviderStuff.Tests/Jellyfin.Plugin.ProviderStuff.Tests.csproj

test: restore
	dotnet test Jellyfin.Plugin.ProviderStuff.Tests/Jellyfin.Plugin.ProviderStuff.Tests.csproj --configuration Release --no-restore /p:Version="$(VERSION)" /p:AssemblyVersion="$(VERSION)" /p:FileVersion="$(VERSION)"

publish: restore
	dotnet publish Jellyfin.Plugin.ProviderStuff/Jellyfin.Plugin.ProviderStuff.csproj --configuration Release --no-restore --output "$(PUBLISH_DIR)" /p:Version="$(VERSION)" /p:AssemblyVersion="$(VERSION)" /p:FileVersion="$(VERSION)"

package: publish
	mkdir -p "$(RELEASE_DIR)"
	cd "$(PUBLISH_DIR)" && zip -9 "../../$(RELEASE_DIR)/$(ASSET)" Jellyfin.Plugin.ProviderStuff.dll

checksum: package
	python scripts/update_manifest.py --manifest manifest.json --archive "$(RELEASE_DIR)/$(ASSET)" --repository "$(GITHUB_REPO)" --version "$(VERSION)" --target-abi "$(TARGET_ABI)"

manifest: checksum

clean:
	dotnet clean Jellyfin.Plugin.ProviderStuff.sln
