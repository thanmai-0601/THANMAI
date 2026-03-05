import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ApiService } from './api';

describe('ApiService', () => {
    let service: ApiService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [ApiService]
        });
        service = TestBed.inject(ApiService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should be injectable', () => {
        expect(service instanceof ApiService).toBeTrue();
    });

    it('should have get method', () => {
        expect(service.get).toBeDefined();
    });

    it('should have post method', () => {
        expect(service.post).toBeDefined();
    });

    it('should have put method', () => {
        expect(service.put).toBeDefined();
    });

    it('should have delete method', () => {
        expect(service.delete).toBeDefined();
    });
});
