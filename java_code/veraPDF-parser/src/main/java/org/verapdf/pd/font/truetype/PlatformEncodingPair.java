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
package org.verapdf.pd.font.truetype;

/**
 * Represents pair of platform and encoding values for cmap table.
 *
 *
 * <h2>.NET porting notes</h2>
 * In C#, make {@code PlatformEncodingPair} a compact readonly record/struct over its numeric
 * range or encoded table data. Preserve PDF/CMap lookup boundaries and binary table byte order;
 * expose collection views as {@code IReadOnlyList<T>} rather than leaking parser-owned arrays.
 *
 * @author Sergey Shemyakov
 */
class PlatformEncodingPair {

    private final int platformID;
    private final int encodingID;

    PlatformEncodingPair(int platformID, int encodingID) {
        this.platformID = platformID;
        this.encodingID = encodingID;
    }

    int getPlatformID() {
        return platformID;
    }

    int getEncodingID() {
        return encodingID;
    }
}
