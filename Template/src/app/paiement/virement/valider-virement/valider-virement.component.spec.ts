import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValiderVirementComponent } from './valider-virement.component';

describe('ValiderVirementComponent', () => {
  let component: ValiderVirementComponent;
  let fixture: ComponentFixture<ValiderVirementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ValiderVirementComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ValiderVirementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
