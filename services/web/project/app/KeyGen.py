from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.asymmetric import rsa, padding
from cryptography.hazmat.primitives import serialization, hashes


# TODO: integrate key gen into Agent gen

def makeKeys(outDir):
    try:
        # CREATE PRIVATE KEY
        pri_key = rsa.generate_private_key(public_exponent=65537,
                                           key_size=2048,
                                           backend=default_backend())
        # CREATE PEM
        pem = pri_key.private_bytes(encoding=serialization.Encoding.PEM,
                                    format=serialization.PrivateFormat.PKCS8,
                                    encryption_algorithm=serialization.NoEncryption())
        # WRITE PRI KEY TO FILE
        with open(outDir + '/private_key.pem', 'wb') as f:
            f.write(pem)
        # GET PUB KEY AND PEM
        pub_key = pri_key.public_key()
        pem = pub_key.public_bytes(encoding=serialization.Encoding.PEM,
                                   format=serialization.PublicFormat.SubjectPublicKeyInfo)
        # WRITE PUB KEY TO OUTPUT DIR
        with open(outDir + '/public_key.pem', 'wb') as f:
            f.write(pem)
    except Exception as e:
        print(str(e))
        return False
    return True


def decrypt(pri_key, encrypted_message):
    with open(pri_key, 'rb') as key_file:
        private_key = serialization.load_pem_private_key(key_file.read(),
                                                         password=None,
                                                         backend=default_backend())
    decrypted = private_key.decrypt(encrypted_message,
                                    padding.OAEP(mgf=padding.MGF1(algorithm=hashes.SHA256()),
                                                 algorithm=hashes.SHA256(),
                                                 label=None))
    return decrypted


def encrypt(pub_key, unencrypted_message):
    with open(pub_key, 'rb') as key_file:
        public_key = serialization.load_pem_public_key(key_file.read(),
                                                       backend=default_backend())
    encrypted = public_key.encrypt(unencrypted_message,
                                   padding.OAEP(mgf=padding.MGF1(algorithm=hashes.SHA256()),
                                                algorithm=hashes.SHA256(),
                                                label=None))
    return encrypted


if __name__ == "__main__":
    keyDir = "/home/app/web/project/app/keystore"
    created = makeKeys(keyDir)
    if created:
        message = b"Please encrypt me"
        encrypted = encrypt(keyDir + "/public_key.pem", message)
        print("\nThe encrypted message is:\n {0}".format(encrypted))
        decrypted = decrypt(keyDir + "/private_key.pem", encrypted)
        print("\nThe decrypted message is:\n {0}\n".format(decrypted))
