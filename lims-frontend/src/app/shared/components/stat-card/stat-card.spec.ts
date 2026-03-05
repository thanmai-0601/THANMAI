import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { StatCard } from './stat-card';

describe('StatCard', () => {
    let component: StatCard;
    let fixture: ComponentFixture<StatCard>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [StatCard],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
        fixture = TestBed.createComponent(StatCard);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should have default label as empty string', () => {
        expect(component.label).toBe('');
    });

    it('should have default value as 0', () => {
        expect(component.value).toBe(0);
    });

    it('should have default icon', () => {
        expect(component.icon).toBe('📊');
    });

    it('should accept custom inputs', () => {
        component.label = 'Total Policies';
        component.value = 150;
        component.icon = '📋';
        component.prefix = '₹';
        fixture.detectChanges();
        expect(component.label).toBe('Total Policies');
        expect(component.value).toBe(150);
        expect(component.icon).toBe('📋');
        expect(component.prefix).toBe('₹');
    });
});
