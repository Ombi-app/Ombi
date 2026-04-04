import { describe, it, expect, beforeEach, vi } from 'vitest';
import { StorageService } from './storage-service';

describe('StorageService', () => {
  let service: StorageService;

  beforeEach(() => {
    service = new StorageService();
    localStorage.clear();
  });

  it('should save and retrieve a value', () => {
    service.save('testKey', 'testValue');
    expect(service.get('testKey')).toBe('testValue');
  });

  it('should return null for a non-existent key', () => {
    expect(service.get('nonExistent')).toBeNull();
  });

  it('should remove a value', () => {
    service.save('testKey', 'testValue');
    service.remove('testKey');
    expect(service.get('testKey')).toBeNull();
  });

  it('should overwrite existing value on save', () => {
    service.save('key', 'first');
    service.save('key', 'second');
    expect(service.get('key')).toBe('second');
  });

  it('should handle empty string values', () => {
    service.save('key', '');
    expect(service.get('key')).toBe('');
  });
});
