FROM mcr.microsoft.com/dotnet/aspnet:9.0

# python3: computeScript evaluation
# fontconfig + fonts: Spire.Doc Word→PDF resolves fonts via system/fontconfig on Linux (otherwise "Cannot found font installed on the system")
# Times New Roman is not shipped on Linux; Tinos (croscore) + Liberation Serif substitute it for Vietnamese Latin.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        python3 \
        libgdiplus \
        libreoffice-writer \
        fontconfig \
        fonts-dejavu-core \
        fonts-freefont-ttf \
        fonts-liberation \
        fonts-noto-core \
        fonts-noto-extra \
        fonts-croscore \
        fonts-crosextra-carlito \
        fonts-crosextra-caladea \
    && printf '%s\n' \
        '<?xml version="1.0"?>' \
        '<!DOCTYPE fontconfig SYSTEM "fonts.dtd">' \
        '<fontconfig>' \
        '  <alias binding="same">' \
        '    <family>Times New Roman</family>' \
        '    <prefer>' \
        '      <family>Tinos</family>' \
        '      <family>Liberation Serif</family>' \
        '      <family>Noto Serif</family>' \
        '    </prefer>' \
        '  </alias>' \
        '  <alias binding="same">' \
        '    <family>Times</family>' \
        '    <prefer>' \
        '      <family>Tinos</family>' \
        '      <family>Liberation Serif</family>' \
        '    </prefer>' \
        '  </alias>' \
        '</fontconfig>' \
        > /etc/fonts/conf.d/69-ione-times-new-roman.conf \
    && fc-cache -f \
    && rm -rf /var/lib/apt/lists/*

ENV IONE_PYTHON=/usr/bin/python3

COPY bin/Release/net9.0/publish/ app/
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "iOne.HttpApi.Host.dll"]