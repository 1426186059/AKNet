#include "boringssl_wrapper.h"

EVP_CIPHER_CTX* KNet_EVP_CIPHER_CTX_new()
{
	return EVP_CIPHER_CTX_new();
}

int KNet_EVP_EncryptInit_ex(EVP_CIPHER_CTX* ctx, const EVP_CIPHER* cipher, ENGINE* impl,
    const unsigned char* key, const unsigned char* iv)
{
	return EVP_EncryptInit_ex(ctx, cipher, impl, key, iv);
}

int KNet_EVP_EncryptUpdate(EVP_CIPHER_CTX* ctx, unsigned char* out,
	int* outl, const unsigned char* in, int inl)
{
	return EVP_EncryptUpdate(ctx, out, outl, in, inl);
}