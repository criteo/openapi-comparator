#!/bin/bash
# Run from the root of the repository, with target version to prepare as argument
_version=$1
for csproj in ./src/**/*.csproj; do
    echo "${csproj}"
    sed "s@<Version>[0-9\.]\+</Version>@<Version>${_version}</Version>@g" "${csproj}"
done

git ./src/**/*.csproj && git commit -v --edit -m "Bump version to ${_version}" && \
echo "Tagging version: '${_version}'"
git tag -a "v${_version}" -m "Releasing version ${_version}"
echo "Pushing tag: 'v${_version}'"
git push origin "v${_version}"