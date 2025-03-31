#!/bin/bash
# Run from the root of the repository, with target version to prepare as argument
_version=$1

echo "Updating .csproj..."
for csproj in ./src/**/*.csproj; do
    echo "${csproj}"
    sed -i "s@<Version>[0-9\.]\+</Version>@<Version>${_version}</Version>@g" "${csproj}"
done

git add ./src/**/*.csproj && \
git commit -v --edit -m "Bump to ${_version}" && \
git tag "v${_version}" && \
echo "Tagging version: '${_version}'"
git push origin master --tags