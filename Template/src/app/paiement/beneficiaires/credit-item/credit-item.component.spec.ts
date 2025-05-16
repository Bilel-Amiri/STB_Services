import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BeneficiaireItemComponent } from './credit-item.component';

describe('BeneficiaireItemComponent', () => {
  let component: BeneficiaireItemComponent;
  let fixture: ComponentFixture<BeneficiaireItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BeneficiaireItemComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BeneficiaireItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
