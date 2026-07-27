#include "boringssl_wrapper.h"

SSL_CTX* KNet_SSL_CTX_new()
{
	return SSL_CTX_new(TLS_method());
}

int KNet_SSL_CTX_set_min_proto_version(SSL_CTX* ctx, uint16_t version)
{
	return SSL_CTX_set_min_proto_version(ctx, version);
}

int KNet_SSL_CTX_set_max_proto_version(SSL_CTX* ctx, uint16_t version)
{
	return SSL_CTX_set_max_proto_version(ctx, version);
}

int KNet_SSL_CTX_set_ciphersuites(SSL_CTX* ctx, const char* str)
{
	return SSL_CTX_set_ciphersuites(ctx, str);
}

int KNet_SSL_CTX_set_default_verify_paths(SSL_CTX* ctx)
{
	return SSL_CTX_set_default_verify_paths(ctx);
}

int KNet_SSL_CTX_set_quic_method(SSL_CTX* ctx, SSL_QUIC_METHOD* meths)
{
	return SSL_CTX_set_quic_method(ctx, meths);
}

int KNet_SSL_provide_quic_data(SSL* ssl, enum ssl_encryption_level_t level, const uint8_t* data, size_t len)
{
	return SSL_provide_quic_data(ssl, level, data, len);
}

void* KNet_SSL_get_app_data(const SSL* ssl)
{
	return SSL_get_app_data(ssl);
}

const SSL_CIPHER* KNet_SSL_get_current_cipher(const SSL* ssl)
{
	return SSL_get_current_cipher(ssl);
}

uint32_t KNet_SSL_CIPHER_get_id(const SSL_CIPHER* cipher)
{
	return SSL_CIPHER_get_id(cipher);
}

long KNet_SSL_CTX_set_session_cache_mode(SSL_CTX* ctx, long m)
{
	return SSL_CTX_set_session_cache_mode(ctx, m);
}

void KNet_SSL_CTX_sess_set_new_cb(SSL_CTX* ctx, int (*new_session_cb) (struct ssl_st* ssl, SSL_SESSION* sess))
{
	 SSL_CTX_sess_set_new_cb(ctx, new_session_cb);
}

//-------------------------------------------------------------------------------
BIO* KNet_BIO_new()
{
	return BIO_new(BIO_s_mem());
}

int KNet_BIO_free(BIO* bio)
{
	return BIO_free(bio);
}

long KNet_BIO_get_mem_data(BIO* bio, void* Data)
{
	return BIO_get_mem_data(bio, Data);
}

BIO* KNet_BIO_new_mem_buf(const void* buf, int len)
{
	return BIO_new_mem_buf(buf, len);
}

//-------------------------------------------------------------------------------------------------------

int KNet_PEM_write_bio_SSL_SESSION(BIO* bio, SSL_SESSION* session)
{
	return PEM_write_bio_SSL_SESSION(bio, session);
}

SSL_SESSION* KNet_PEM_read_bio_SSL_SESSION(BIO* bio, SSL_SESSION** session, pem_password_cb* cb, void* u)
{
	return PEM_read_bio_SSL_SESSION(bio, session, cb, u);
}

int KNet_SSL_set_session(SSL* to, SSL_SESSION* session)
{
	return SSL_set_session(to, session);
}

SSL_SESSION* KNet_SSL_get_session(SSL* ssl)
{
	return SSL_get_session(ssl);
}

void KNet_SSL_SESSION_free(SSL_SESSION* session)
{
	SSL_SESSION_free(session);
}

void KNet_SSL_set_quic_use_legacy_codepoint(SSL* ssl, int use_legacy)
{
	SSL_set_quic_use_legacy_codepoint(ssl, use_legacy);
}

int KNet_SSL_set_quic_transport_params(SSL* ssl, const uint8_t* params, size_t params_len)
{
	return SSL_set_quic_transport_params(ssl, params, params_len);
}

void KNet_SSL_get_peer_quic_transport_params(SSL* ssl, const uint8_t** params, size_t* params_len)
{
	SSL_get_peer_quic_transport_params(ssl, params, params_len);
}

int KNet_SSL_SESSION_set1_ticket_appdata(SSL_SESSION* session, void* data, int nLength)
{
	return SSL_SESSION_set1_ticket_appdata(session, data, nLength);
}

int KNet_SSL_process_quic_post_handshake(SSL* ssl)
{
	return SSL_process_quic_post_handshake(ssl);
}

int KNet_SSL_new_session_ticket(SSL* ssl)
{
	return SSL_new_session_ticket(ssl);
}

int KNet_SSL_do_handshake(SSL* ssl)
{
	return SSL_do_handshake(ssl);
}

int KNet_SSL_session_reused(SSL* ssl)
{
	return SSL_session_reused(ssl);
}

int KNet_SSL_get_early_data_status(SSL* ssl)
{
	return SSL_get_early_data_status(ssl);
}

void KNet_SSL_get0_alpn_selected(const SSL* ssl, const unsigned char** data, unsigned int* len)
{
	SSL_get0_alpn_selected(ssl, data, len);
}

int KNet_SSL_get_error(SSL* ssl, int ret_code)
{
	return SSL_get_error(ssl, ret_code);
}

unsigned long KNet_ERR_get_error()
{
	return ERR_get_error();
}

//----------------------------------------------------------------------------------------------------

SSL* KNet_SSL_new(SSL_CTX* ctx)
{
	return SSL_new(ctx);
}

int KNet_SSL_set_app_data(SSL* ssl, void* AppData)
{
	return SSL_set_app_data(ssl, AppData);
}

void KNet_SSL_set_accept_state(SSL* ssl)
{
	SSL_set_accept_state(ssl);
}

void KNet_SSL_set_connect_state(SSL* ssl)
{
	SSL_set_connect_state(ssl);
}

long KNet_SSL_set_tlsext_host_name(SSL* ssl, char* url)
{
	return SSL_set_tlsext_host_name(ssl, url);
}

int KNet_SSL_set_alpn_protos(SSL* ssl, const unsigned char* protos, unsigned int protos_len)
{
	return SSL_set_alpn_protos(ssl, protos, protos_len);
}

void KNet_SSL_set_quic_early_data_enabled(SSL* ssl, int enabled)
{
	SSL_set_quic_early_data_enabled(ssl, enabled);
}


//-------------------------------------֤��------------------------------------------
int KNet_SSL_CTX_set_max_early_data(SSL_CTX* ctx, uint32_t max_early_data)
{
	return SSL_CTX_set_max_early_data(ctx, max_early_data);
}

int KNet_SSL_CTX_set_session_ticket_cb(SSL_CTX* ctx, SSL_CTX_generate_session_ticket_fn gen_cb,
	SSL_CTX_decrypt_session_ticket_fn dec_cb, void* arg)
{
	return SSL_CTX_set_session_ticket_cb(ctx, gen_cb, dec_cb, arg);
}

int KNet_SSL_CTX_set_num_tickets(SSL_CTX* ctx, size_t num_tickets)
{
	return SSL_CTX_set_num_tickets(ctx, num_tickets);
}

void KNet_SSL_CTX_set_default_passwd_cb_userdata(SSL_CTX* ctx, void* u)
{
	SSL_CTX_set_default_passwd_cb_userdata(ctx, u);
}

int KNet_SSL_CTX_use_PrivateKey_file(SSL_CTX* ctx, const char* file, int type)
{
	return SSL_CTX_use_PrivateKey_file(ctx, file, type);
}

int KNet_SSL_CTX_use_certificate_chain_file(SSL_CTX* ctx, const char* file)
{
	return SSL_CTX_use_certificate_chain_file(ctx, file);
}

int KNet_SSL_CTX_use_PrivateKey(SSL_CTX* ctx, EVP_PKEY* pkey)
{
	return SSL_CTX_use_PrivateKey(ctx, pkey);
}

int KNet_SSL_CTX_use_certificate(SSL_CTX* ctx, X509* x)
{
	return SSL_CTX_use_certificate(ctx, x);
}

long KNet_BIO_set_mem_eof_return(BIO* bp, long larg)
{
	return BIO_set_mem_eof_return(bp, larg);
}

int KNet_BIO_write(BIO* b, const void* data, int dlen)
{
	return BIO_write(b, data, dlen);
}

long KNet_SSL_CTX_add_extra_chain_cert(SSL_CTX* ctx, void* parg)
{
	return SSL_CTX_add_extra_chain_cert(ctx, parg);
}

int KNet_SSL_CTX_check_private_key(SSL_CTX* ctx)
{
	return SSL_CTX_check_private_key(ctx);
}

int KNet_SSL_CTX_load_verify_locations(SSL_CTX* ctx, const char* CAfile, const char* CApath)
{
	return SSL_CTX_load_verify_locations(ctx, CAfile, CApath);
}

void KNet_SSL_CTX_set_cert_verify_callback(SSL_CTX* ctx, int (*cb) (X509_STORE_CTX*, void*), void* arg)
{
	SSL_CTX_set_cert_verify_callback(ctx, cb, arg);
}

void KNet_SSL_CTX_set_verify(SSL_CTX* ctx, int mode, SSL_verify_cb callback)
{
	SSL_CTX_set_verify(ctx, mode, callback);
}

void KNet_SSL_CTX_set_verify_depth(SSL_CTX* ctx, int depth)
{
	SSL_CTX_set_verify_depth(ctx, depth);
}

uint64_t KNet_SSL_CTX_set_options(SSL_CTX* ctx, uint64_t op)
{
	return SSL_CTX_set_options(ctx, op);
}

uint64_t KNet_SSL_CTX_clear_options(SSL_CTX* ctx, uint64_t op)
{
	return SSL_CTX_clear_options(ctx, op);
}

long KNet_SSL_CTX_set_mode(SSL_CTX* ctx, long op)
{
	return SSL_CTX_set_mode(ctx, op);
}

void KNet_SSL_CTX_set_alpn_select_cb(SSL_CTX* ctx, SSL_CTX_alpn_select_cb_func cb, void* arg)
{
	SSL_CTX_set_alpn_select_cb(ctx, cb, arg);
}

void KNet_SSL_CTX_set_client_hello_cb(SSL_CTX* ctx, SSL_client_hello_cb_fn cb, void* arg)
{
	SSL_CTX_set_client_hello_cb(ctx, cb, arg);
}

void KNet_EVP_PKEY_free(EVP_PKEY* pkey)
{
	EVP_PKEY_free(pkey);
}

int KNet_SSL_SESSION_get0_ticket_appdata(SSL_SESSION* ss, void** data, size_t* len)
{
	return SSL_SESSION_get0_ticket_appdata(ss, data, len);
}

int KNet_SSL_client_hello_get0_ext(SSL* s, unsigned int type, const unsigned char** out, size_t* outlen)
{
	return SSL_client_hello_get0_ext(s, type, out, outlen);
}


/// <summary>
/// ֤�����
/// </summary>
/// <param name="sk"></param>

void KNet_sk_X509_free(struct stack_st_X509* sk)
{
	sk_X509_free(sk);
}

void KNet_X509_free(X509* x)
{
	X509_free(x);
}

PKCS12* KNet_d2i_PKCS12_bio(BIO* bp, PKCS12** p12)
{
	return d2i_PKCS12_bio(bp, p12);
}

int KNet_PKCS12_parse(PKCS12* p12, const char* pass, EVP_PKEY** pkey, X509** cert, STACK_OF(X509)** ca)
{
	return PKCS12_parse(p12, pass, pkey, cert, ca);
}

void KNet_PKCS12_free(PKCS12* p12)
{
	PKCS12_free(p12);
}

X509* KNet_sk_X509_pop(struct stack_st_X509* st)
{
	return sk_X509_pop(st);
}




X509* KNet_X509_STORE_CTX_get0_cert(const X509_STORE_CTX* ctx)
{
	return X509_STORE_CTX_get0_cert(ctx);
}

void* KNet_X509_STORE_CTX_get_ex_data(const X509_STORE_CTX* ctx)
{
	return X509_STORE_CTX_get_ex_data(ctx, SSL_get_ex_data_X509_STORE_CTX_idx());
}

int KNet_X509_verify_cert(X509_STORE_CTX* ctx)
{
	return X509_verify_cert(ctx);
}

void KNet_X509_STORE_CTX_set_error(X509_STORE_CTX* ctx, int s)
{
	X509_STORE_CTX_set_error(ctx, s);
}

int KNet_X509_STORE_CTX_get_error(const X509_STORE_CTX* ctx)
{
	return X509_STORE_CTX_get_error(ctx);
}

void KNet_OPENSSL_free(void* ptr)
{
	 OPENSSL_free(ptr);
}

int KNet_i2d_X509(const X509* x, unsigned char ** outBuf)
{
	 return i2d_X509(x, outBuf);
}

int KNet_i2d_PKCS7(const PKCS7* x, unsigned char** outBuf)
{
	return i2d_PKCS7(x, outBuf);
}

struct stack_st_X509* KNet_X509_STORE_CTX_get0_chain(const X509_STORE_CTX* ctx)
{
	return X509_STORE_CTX_get0_chain(ctx);
}

int KNet_sk_X509_num(struct stack_st_X509* ctx)
{
	return sk_X509_num(ctx);
}

PKCS7* KNet_PKCS7_new()
{
	return PKCS7_new();
}

void KNet_PKCS7_free(PKCS7* a)
{
	PKCS7_free(a);
}

int KNet_PKCS7_set_type(PKCS7* p7, int type)
{
	return PKCS7_set_type(p7, type);
}

int KNet_PKCS7_content_new(PKCS7* p7, int nid)
{
	return PKCS7_content_new(p7, nid);
}

int KNet_PKCS7_add_certificate(PKCS7* p7, X509* x)
{
	return PKCS7_add_certificate(p7, x);
}

X509* KNet_sk_X509_value(struct stack_st_X509* sk, int idx)
{
	return sk_X509_value(sk, idx);
}

void KNet_SSL_CTX_free(SSL_CTX* x)
{
	SSL_CTX_free(x);
}

