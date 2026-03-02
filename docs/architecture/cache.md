# Cache

## Provider
- Redis is the primary distributed cache provider.
- Valkey is a drop-in alternative; you can switch by pointing `ConnectionStrings:Redis` to a Valkey instance.

## TTL rules
- Every entry must be written with an explicit TTL.
- Short-lived API response caching should use small TTLs (for example, 30 seconds).
- Use absolute expiration to keep cache freshness predictable across nodes.

## Stampede note
- The template currently uses a cache-aside pattern.
- Under high concurrency, multiple misses can compute the same value at once (cache stampede).
- For hot keys, consider adding request coalescing, locking, or jittered early refresh.
