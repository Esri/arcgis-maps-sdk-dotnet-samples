FROM alpine:3.12
LABEL author="Hamish Duff <hduff@esri.com>"
ENV PYTHONUNBUFFERED=1
# Add scripts for the check.
ADD entry.py /entry.py
ADD sample_sync.py /sample_sync.py
ADD process_metadata.py /process_metadata.py
ADD readme_copy.py /readme_copy.py
# Install dependencies.
RUN echo "**** Install Python ****" && \
    apk add --no-cache python3 && \
    if [ ! -e /usr/bin/python ]; then ln -sf python3 /usr/bin/python ; fi
ENTRYPOINT ["python3", "/entry.py"]