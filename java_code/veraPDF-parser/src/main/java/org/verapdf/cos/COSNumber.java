/*
 * This file is part of veraPDF Parser, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Parser is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Parser as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Parser as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.cos;

import java.math.BigDecimal;

/**
 * Base for PDF integer and real values, with decimal access for comparisons
 * that should not depend on primitive floating-point equality.
 *
 * <h2>.NET porting notes</h2>
 * In C#, port {@code COSNumber} as an immutable COS value, preferably a sealed class or readonly
 * record/record struct with PDF-specific equality. Keep PDF {@code null} distinct from C# {@code
 * null}, preserve integer/real/string representations where syntax fidelity matters, and avoid
 * using a broad {@code object} value property.
 */
public abstract class COSNumber extends COSDirect {

    public COSNumber() {
        super();
    }

    /**
     * Returns this number as a decimal value.
     *
     * @return decimal representation of this COS number
     */
    public abstract BigDecimal getDecimalValue();

}
