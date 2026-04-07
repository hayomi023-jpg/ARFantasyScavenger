#!/bin/bash
# Generate Keystore for Unity Android Build
# Run this script in Git Bash, MingW64, or WSL

# Configuration
KEYSTORE_NAME="arfantasy.keystore"
KEY_ALIAS="arfantasykey"
KEYSTORE_PASS="ardfantasy123"
KEY_PASS="ardfantasy123"
VALIDITY_DAYS=10000

echo "=========================================="
echo "Unity Android Keystore Generator"
echo "=========================================="

# Check if keytool is available
if ! command -v keytool &> /dev/null; then
    echo "ERROR: keytool not found!"
    echo "Please install JDK and add it to PATH"
    echo "Or run this in Android Studio's terminal"
    exit 1
fi

# Create keystore directory if not exists
mkdir -p keystore

# Generate keystore
keytool -genkeypair \
    -v \
    -storetype PKCS12 \
    -keystore "keystore/$KEYSTORE_NAME" \
    -alias $KEY_ALIAS \
    -keyalg RSA \
    -keysize 2048 \
    -validity $VALIDITY_DAYS \
    -storepass $KEYSTORE_PASS \
    -keypass $KEY_PASS \
    -dname "CN=AR Fantasy Scavenger, OU=GameDev, O=AR Fantasy, L=City, ST=State, C=US"

if [ $? -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "SUCCESS! Keystore created at:"
    echo "keystore/$KEYSTORE_NAME"
    echo "=========================================="
    echo ""
    echo "UNITY PLAYER SETTINGS:"
    echo "  Keystore: keystore/$KEYSTORE_NAME"
    echo "  Keystore Password: $KEYSTORE_PASS"
    echo "  Key Alias: $KEY_ALIAS"
    echo "  Key Password: $KEY_PASS"
    echo ""
    echo "=========================================="
else
    echo "ERROR: Failed to create keystore"
    exit 1
fi
