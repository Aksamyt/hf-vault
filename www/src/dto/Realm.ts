enum Impl {
  FR,
  EN,
  ES,
}

function implOfString(s: string): Impl|null {
  const map: Record<string, Impl> = {
    FR: Impl.FR,
    EN: Impl.EN,
    ES: Impl.ES,
  };
  return map[s] ?? null;
}

function stringOfImpl(i: Impl): string {
  switch (i) {
    case Impl.EN: return 'EN';
    case Impl.ES: return 'ES';
    case Impl.FR:
    default: return 'FR';
  }
}

export default class Realm {
  constructor(
    public value: Impl,
  ) {}

  static fromJSON(o: any): Realm|null {
    if (o != null && typeof o === 'object') {
      for (const [k, v] of Object.entries(o)) {
        if (v == null) {
          const maybeImpl = implOfString(k);
          if (maybeImpl != null) {
            return new Realm(maybeImpl);
          }
        }
      }
    }
    return null;
  }

  toJSON() {
    return { [stringOfImpl(this.value)]: null };
  }

  getFlag(): string {
    switch (this.value) {
      case Impl.EN: return '🏴󠁧󠁢󠁥󠁮󠁧󠁿';
      case Impl.ES: return '🇪🇸';
      case Impl.FR:
      default: return '🇫🇷';
    }
  }

  toString(): string {
    return stringOfImpl(this.value);
  }
}
