@echo off
REM Generate Keystore for Unity Android Build
REM Run this in Command Prompt or PowerShell

REM Configuration
set KEYSTORE_NAME=arfantasy.keystore
set KEY_ALIAS=arfantasykey
set KEYSTORE_PASS=ardfantasy123
set KEY_PASS=ardfantasy123
set VALIDITY_DAYS=10000

echo ==========================================
echo Unity Android Keystore Generator
echo ==========================================

REM Check if keytool is available
where keytool >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: keytool not found!
    echo Please install JDK and add it to PATH
    exit /b 1
)

REM Create keystore directory if not exists
if not exist "keystore" mkdir keystore

REM Generate keystore
keytool -genkeypair -v -storetype PKCS12 -keystore "keystore\%KEYSTORE_NAME%" -alias %KEY_ALIAS% -keyalg RSA -keysize 2048 -validity %VALIDITY_DAYS% -storepass %KEYSTORE_PASS% -keypass %KEY_PASS% -dname "CN=AR Fantasy Scavenger, OU=GameDev, O=AR Fantasy, L=City, ST=State, C=US"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ==========================================
    echo SUCCESS! Keystore created at:
    echo keystore\%KEYSTORE_NAME%
    echo ==========================================
    echo.
    echo UNITY PLAYER SETTINGS:
    echo   Keystore: keystore\%KEYSTORE_NAME%
    echo   Keystore Password: %KEYSTORE_PASS%
    echo   Key Alias: %KEY_ALIAS%
    echo   Key Password: %KEY_PASS%
    echo.
    echo ==========================================
) else (
    echo ERROR: Failed to create keystore
    exit /b 1
)
